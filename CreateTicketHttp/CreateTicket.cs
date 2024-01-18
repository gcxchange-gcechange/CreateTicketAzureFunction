using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace CreateTicketHttp
{
    public interface ITicketWrapper
    {
        Task<object> createTicket(string email);
    }

    public class TicketClientMock : ITicketWrapper
    {
        private readonly string _result;

        public TicketClientMock(string result)
        {
            _result = result;
        }

        public async Task<object> createTicket(string email)
        {
            var mockResult = Task<object>.Run(() => { return _result; });
            return await mockResult;
        }
    }

    public static class CreateTicket
    {
        public static ITicketWrapper _ticketClientWrapper;

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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            HttpResponseMessage response = new();

            try
            {
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
                    if (formItem.name == "attachment")
                    {
                        attachments.Add(String.Concat(formItem.name, fileCount.ToString()), formItem);
                        fileCount++;
                    }
                    else
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


                if (email != null)
                {
                    var result = CreateTicketAPI(_ticketClientWrapper, log, email, reasonOneVal, reasonTwoVal, ticketDescription, pageURL, startDate, endDate, emailTo, isOngoing, attachmentName, attachmentType, attachmentData);

                    if (result.Result != null)
                    {
                        response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.ReasonPhrase = "Finished";
                    }
                    else
                    {
                        response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        response.ReasonPhrase = "E1BadRequest";
                    }
                }
                else
                {
                    response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = "E0NoUserEmail";
                }
            }
            catch (Exception e)
            {
                log.LogError($"Message: {e.Message}");
                if (e.InnerException is not null) log.LogError($"InnerException: {e.InnerException.Message}");
                log.LogError($"StackTrace: {e.StackTrace}");
            }

            return response;
        }

        public static async Task<object> CreateTicketAPI(ITicketWrapper _ticketClientWrapper, ILogger log, string UserEmail, string reasonOne, string reasonTwo, string description, string pageUrl, string startDate, string endDate, string sendReportTo, string isOngoing, Dictionary<string, string> attachmentName, Dictionary<string, string> attachmentType, Dictionary<string, byte[]> attachmentData)
        {
            if (_ticketClientWrapper != null)
            {
                return _ticketClientWrapper.createTicket(UserEmail);
            }

            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();

            string fdDomain = config["fdDomain"];
            string APIKey = Auth.GetAPIKey(log);
            string path = "/api/v2/tickets";
            string url = "https://" + fdDomain + ".freshdesk.com" + path;
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(APIKey + ":X")); // It could be your username:password also.

            HttpClient sharedClient = new()
            {
                BaseAddress = new Uri(url)
            };
            sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            sharedClient.DefaultRequestHeaders.ConnectionClose = false; // aka Keep-Alive

            HttpRequestMessage request = new();
            request.Headers.Clear();
            request.Method = HttpMethod.Post;
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var content = new MultipartFormDataContent
            {
                { new StringContent(UserEmail), "\"email\"" },
                { new StringContent(reasonOne), "\"subject\"" },
                { new StringContent("2"), "\"status\"" },
                { new StringContent("1"), "\"priority\"" },
                { new StringContent(description), "\"description\"" },            // if not null ?
                { new StringContent(reasonOne), "\"custom_fields[cf_reason]\"" },
                { new StringContent(reasonTwo), "\"custom_fields[cf_reason_2]\"" },
                { new StringContent(pageUrl), "\"custom_fields[cf_page_url]\"" },
                { new StringContent(startDate), "\"custom_fields[cf_start_date]\"" },
                { new StringContent(endDate), "\"custom_fields[cf_end_date]\"" },
                { new StringContent(sendReportTo), "\"custom_fields[cf_send_report_to]\"" },
                { new StringContent(isOngoing), "\"custom_fields[cf_ongoing]\"" }
,            };

            if (attachmentData.Count > 0)
            {
                log.LogInformation("Attaching files...");
                foreach (KeyValuePair<string, byte[]> files in attachmentData)
                {
                    content.Add(new StreamContent(new MemoryStream(files.Value)), attachmentType[files.Key], attachmentName[files.Key]);
                }
            } else
            {
                log.LogInformation("No files to attach");
            }

            request.Content = content;

            try
            {
                log.LogInformation("Submitting Request");
                var response = await sharedClient.PostAsync(url, content);
                log.LogInformation($"StatusCode: {response.StatusCode}");
                return Task.FromResult<object>(response);
            }
            catch (WebException ex)
            {
                log.LogInformation("API Error: Your request is not successful. If you are not able to debug this error properly, mail us at support@freshdesk.com with the follwing X-Request-Id");
                log.LogInformation("X-Request-Id: {0}", ex.Response.Headers["X-Request-Id"]);
                log.LogInformation("Error Status Code : {1} {0}", ((HttpWebResponse)ex.Response).StatusCode, (int)((HttpWebResponse)ex.Response).StatusCode);

                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    log.LogInformation("Error Response: ");
                    log.LogInformation(reader.ReadToEnd());
                    return Task.FromResult<object>(null);
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("ERROR");
                log.LogInformation(ex.Message);
                log.LogInformation(ex.StackTrace);
                return Task.FromResult<object>(null);
            }
        }
    }
}