using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CreateTicketHttp
{
    public static class CreateTicket
    {
        private static void WriteCRLF(Stream o)
        {
            byte[] crLf = Encoding.ASCII.GetBytes("\r\n");
            o.Write(crLf, 0, crLf.Length);
        }

        private static void WriteBoundaryBytes(Stream o, string b, bool isFinalBoundary)
        {
            string boundary = isFinalBoundary == true ? "--" + b + "--" : "--" + b + "\r\n";
            byte[] d = Encoding.ASCII.GetBytes(boundary);
            o.Write(d, 0, d.Length);
        }

        private static void WriteContentDispositionFormDataHeader(Stream o, string name)
        {
            string data = "Content-Disposition: form-data; name=\"" + name + "\"\r\n\r\n";
            byte[] b = Encoding.ASCII.GetBytes(data);
            o.Write(b, 0, b.Length);
        }

        private static void WriteContentDispositionFileHeader(Stream o, string name, string fileName, string contentType)
        {
            string data = "Content-Disposition: form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"\r\n";
            data += "Content-Type: " + contentType + "\r\n\r\n";
            byte[] b = Encoding.ASCII.GetBytes(data);
            o.Write(b, 0, b.Length);
        }

        private static void WriteString(Stream o, string data)
        {
            byte[] b = Encoding.UTF8.GetBytes(data);
            o.Write(b, 0, b.Length);
        }

        private class FormItem
        {
            public FormItem() { }
            public string name { get; set; }
            public byte[] data { get; set; }
            public string fileName { get; set; }
            public string mediaType { get; set; }
            public string value { get { return Encoding.UTF8.GetString(data); } }
            public bool isAFileUpload { get { return !String.IsNullOrEmpty(fileName); } }
        }

        [FunctionName("CreateTicket")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string email = null;
            string ticketDescription = null;
            string reasonOneVal = null;
            string reasonTwoVal = null;
            string pageURL = null;
            string startDate = null;
            string endDate = null;
            string emailTo = null;
            string isOngoing = "false";

            var provider = new MultipartMemoryStreamProvider();
            await req.Content.ReadAsMultipartAsync(provider);

            var formItems = new Dictionary<string, FormItem>();
            var attachments = new Dictionary<string, FormItem>();

            int fileCount = 0;

            // Scan the Multiple Parts 
            foreach (HttpContent contentPart in provider.Contents)
            {
                var formItem = new FormItem();
                var contentDisposition = contentPart.Headers.ContentDisposition;
                formItem.name = contentDisposition.Name.Trim('"');
                formItem.data = await contentPart.ReadAsByteArrayAsync();
                formItem.fileName = String.IsNullOrEmpty(contentDisposition.FileName) ? "" : contentDisposition.FileName.Trim('"');
                formItem.mediaType = contentPart.Headers.ContentType == null ? "" : String.IsNullOrEmpty(contentPart.Headers.ContentType.MediaType) ? "" : contentPart.Headers.ContentType.MediaType;
                if(formItem.name == "attachment")
                {
                    log.Info("file count ------" + formItem.name + fileCount);
                    attachments.Add(String.Concat(formItem.name, fileCount.ToString()), formItem);
                    fileCount++;
                } else
                {
                    formItems.Add(formItem.name, formItem);
                }
            }

            // See what fields have been passed to function
            if (formItems.ContainsKey("email"))
            {
                email = formItems["email"].value;
            }
            if (formItems.ContainsKey("reasonOneVal"))
            {
                reasonOneVal = formItems["reasonOneVal"].value;
            }
            if (formItems.ContainsKey("ticketDescription"))
            {
                ticketDescription = formItems["ticketDescription"].value;
            }
            if (formItems.ContainsKey("reasonTwoVal"))
            {
                reasonTwoVal = formItems["reasonTwoVal"].value;
            }
            if (formItems.ContainsKey("pageURL"))
            {
                pageURL = formItems["pageURL"].value;
            }
            if (formItems.ContainsKey("startDate"))
            {
                startDate = formItems["startDate"].value;
            }
            if (formItems.ContainsKey("endDate"))
            {
                endDate = formItems["endDate"].value;
            }
            if (formItems.ContainsKey("emailTo"))
            {
                emailTo = formItems["emailTo"].value;
            }
            if (formItems.ContainsKey("isOngoing"))
            {
                isOngoing = formItems["isOngoing"].value;
            }

            // Treat attachments differently
            var attachmentName = new Dictionary<string, string>();
            var attachmentType = new Dictionary<string, string>();
            var attachmentData = new Dictionary<string, byte[]>();

            if (attachments.Count > 0)
            {
                foreach (KeyValuePair<string, FormItem> files in attachments)
                {
                    attachmentName.Add(files.Key, files.Value.fileName);
                    attachmentType.Add(files.Key, files.Value.mediaType);
                    attachmentData.Add(files.Key, files.Value.data);
                }
            }

            // Check if email is passed
            // return BadRequest if not present
            if (email != null)
            {
                var result = CreateTicketAPI(log, email, reasonOneVal, reasonTwoVal, ticketDescription, pageURL, startDate, endDate, emailTo, isOngoing, attachmentName, attachmentType, attachmentData);

                if (result.Result == null)
                {
                    return req.CreateResponse(HttpStatusCode.OK, "Finished");
                } else
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "E1BadRequest");
                }
            } else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "E0NoUserEmail");
            }
        }

        public static Task<object> CreateTicketAPI(TraceWriter log, string UserEmail, string reasonOne, string reasonTwo, string description, string pageUrl, string startDate,
            string endDate, string sendReportTo, string isOngoing, Dictionary<string, string> attachmentName, Dictionary<string, string> attachmentType, Dictionary<string, byte[]> attachmentData)
        {
            // Load secret information
            string fdDomain = ConfigurationManager.AppSettings["DOMAIN"];
            string APIKey = ConfigurationManager.AppSettings["API_KEY"];
            string productID = ConfigurationManager.AppSettings["API_KEY"];

            string path = "/api/v2/tickets";
            string url = "https://" + fdDomain + ".freshdesk.com" + path;

            // Define boundary:
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            // Web Request:
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

            wr.Headers.Clear();

            // Method and headers:
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;

            // Basic auth:
            string login = APIKey + ":X"; // It could be your username:password also.
            string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(login));
            wr.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            
           // Body:
           using (var rs = wr.GetRequestStream())
           {
               // Email:
               WriteBoundaryBytes(rs, boundary, false);
               WriteContentDispositionFormDataHeader(rs, "email");
               WriteString(rs, UserEmail);
               WriteCRLF(rs);

               // Subject:
               WriteBoundaryBytes(rs, boundary, false);
               WriteContentDispositionFormDataHeader(rs, "subject");
               WriteString(rs, "Test gcxchange ticket - will delete shortly");
               WriteCRLF(rs);
               
               // Product ID:
               WriteBoundaryBytes(rs, boundary, false);
               WriteContentDispositionFormDataHeader(rs, "product_id");
               WriteString(rs, productID);
               WriteCRLF(rs);

               // Status:
               WriteBoundaryBytes(rs, boundary, false);
               WriteContentDispositionFormDataHeader(rs, "status");
               WriteString(rs, "2");
               WriteCRLF(rs);

               // Priority:
               WriteBoundaryBytes(rs, boundary, false);
               WriteContentDispositionFormDataHeader(rs, "priority");
               WriteString(rs, "1");
               WriteCRLF(rs);

                // Description:
                if (description != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "description");
                    WriteString(rs, description);
                    WriteCRLF(rs);
                }

                // Custom field: Update the custom field name in the following snipped
                // Reason 1
                WriteBoundaryBytes(rs, boundary, false);
                WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_reason]");
                WriteString(rs, reasonOne);
                WriteCRLF(rs);

                
               // Reason 2
               if(reasonTwo != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_reason_2]");
                    WriteString(rs, reasonTwo);
                    WriteCRLF(rs);
                }
               
                // Page URL
                if(pageUrl != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_page_url]");
                    WriteString(rs, pageUrl);
                    WriteCRLF(rs);
                }
                
                // Start date
                if(startDate != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_start_date]");
                    WriteString(rs, startDate);
                    WriteCRLF(rs);
                }
                
                // End date
                if(endDate != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_end_date]");
                    WriteString(rs, endDate);
                    WriteCRLF(rs);
                }
                
                // Forward report to
                if(sendReportTo != null)
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_send_report_to]");
                    WriteString(rs, sendReportTo);
                    WriteCRLF(rs);
                }
                
                // Ongoing request
                if(isOngoing != "false")
                {
                    WriteBoundaryBytes(rs, boundary, false);
                    WriteContentDispositionFormDataHeader(rs, "custom_fields[cf_ongoing]");
                    WriteString(rs, "true");
                    WriteCRLF(rs);
                }
               
                //Attachments
                if(attachmentData.Count > 0)
                {
                    foreach (KeyValuePair<string, byte[]> files in attachmentData)
                    {
                        WriteBoundaryBytes(rs, boundary, false);
                        WriteContentDispositionFileHeader(rs, "attachments[]", attachmentName[files.Key], attachmentType[files.Key]);
                        rs.Write(files.Value, 0, files.Value.Length);
                        WriteCRLF(rs);
                    }
                        
                }

                // End marker:
                WriteBoundaryBytes(rs, boundary, true);

                rs.Close();
               
                // Response processing:
                try
                {
                    Console.WriteLine("Submitting Request");
                    var response = (HttpWebResponse)wr.GetResponse();
                    Stream resStream = response.GetResponseStream();
                    string Response = new StreamReader(resStream, Encoding.ASCII).ReadToEnd();
                    //return status code
                    Console.WriteLine("Status Code: {1} {0}", ((HttpWebResponse)response).StatusCode, (int)((HttpWebResponse)response).StatusCode);
                    //return location header
                    Console.WriteLine("Location: {0}", response.Headers["Location"]);
                    //return the response 
                    Console.Out.WriteLine(Response);
                }
                catch (WebException ex)
                {
                    Console.WriteLine("API Error: Your request is not successful. If you are not able to debug this error properly, mail us at support@freshdesk.com with the follwing X-Request-Id");
                    Console.WriteLine("X-Request-Id: {0}", ex.Response.Headers["X-Request-Id"]);
                    Console.WriteLine("Error Status Code : {1} {0}", ((HttpWebResponse)ex.Response).StatusCode, (int)((HttpWebResponse)ex.Response).StatusCode);
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        Console.Write("Error Response: ");
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message);
                }
            }

            return Task.FromResult<object>(null);
           
        }
    }
}
