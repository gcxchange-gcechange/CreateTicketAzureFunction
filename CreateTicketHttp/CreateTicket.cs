using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

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

    public class ResponseValues
    {
        public string StatusCode { get; set; }
    }

    public class WebResponse
    {
        public ResponseValues Result;

        public WebResponse()
        {
            Result = new ResponseValues();
        }
    }

    public class CreateTicket
    {
        private readonly ILogger<CreateTicket> _logger;

        public CreateTicket(ILogger<CreateTicket> logger)
        {
            _logger = logger;
        }

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

        [Function("CreateTicket")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

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

                var formItems = new Dictionary<string, FormItem>();
                var attachments = new Dictionary<string, FormItem>();
                int fileCount = 0;

                var boundary = MultipartRequestHelper.GetBoundary(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(req.ContentType), 100);
                var reader = new MultipartReader(boundary, req.Body);
                var section = await reader.ReadNextSectionAsync();

                // Scan the Multiple Parts aka sections
                while (section != null)
                {
                    string fileName;
                    FormItem formItem = new FormItem();
                    MemoryStream ms = new MemoryStream();
                    string sectionName;

                    await section.Body.CopyToAsync(ms);

                    if (section.AsFileSection() != null) {
                        fileName = section.AsFileSection().FileName;
                        sectionName = section.AsFileSection().Name;
                    } else if (section.AsFormDataSection() != null) {
                        fileName = "";
                        sectionName = section.AsFormDataSection().Name;
                    } else {
                        fileName = "";
                        sectionName = "";
                    }

                    formItem.name = sectionName.Trim('"');
                    formItem.data = ms.ToArray();
                    formItem.fileName = fileName.Trim('"');

                    //todo - not sure how to get the mediaType, not sure it's necessary
                    //formItem.mediaType = fileSection.Section.ContentType == null ? "" : String.IsNullOrEmpty(contentPart.Headers.ContentType.MediaType) ? "" : contentPart.Headers.ContentType.MediaType;
                    
                    if ((formItem.name == "attachment") && formItem.isAFileUpload)
                    {
                        attachments.Add(String.Concat(formItem.name, fileCount.ToString()), formItem);
                        fileCount++;

                        _logger.LogInformation($"attachment formItem.name: {formItem.fileName}");
                        _logger.LogInformation($"attachment formItem.isAFileUpload: {formItem.isAFileUpload}");
                        _logger.LogInformation($"attachment formItem.mediaType: {formItem.mediaType}");
                        _logger.LogInformation($"attachment formItem.data.Length: {formItem.data.Length}");
                    }
                    else
                    {
                        formItems.Add(formItem.name, formItem);
                    }

                    section = await reader.ReadNextSectionAsync();
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
                    var result = CreateTicketAPI(_ticketClientWrapper, _logger, email, reasonOneVal, reasonTwoVal, ticketDescription, pageURL, startDate, endDate, emailTo, isOngoing, attachmentName, attachmentType, attachmentData);

                    if (result.Result != null && (result.Result.StatusCode == HttpStatusCode.OK || result.Result.StatusCode == HttpStatusCode.Created))
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
                _logger.LogError($"Message: {e.Message}");
                if (e.InnerException is not null) _logger.LogError($"InnerException: {e.InnerException.Message}");
                _logger.LogError($"StackTrace: {e.StackTrace}");
            }

            return response;
        }

        public static async Task<HttpResponseMessage> CreateTicketAPI(ITicketWrapper _ticketClientWrapper, ILogger log, string UserEmail, string reasonOne, string reasonTwo, string description, string pageUrl, string startDate, string endDate, string sendReportTo, string isOngoing, Dictionary<string, string> attachmentName, Dictionary<string, string> attachmentType, Dictionary<string, byte[]> attachmentData)
        {
            //if (_ticketClientWrapper != null)
            //{
            //    return _ticketClientWrapper.createTicket(UserEmail);
            //}

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

            log.LogInformation($"UserEmail : {UserEmail}");
            log.LogInformation($"reasonOne : {reasonOne}");
            log.LogInformation($"description : {description}");
            log.LogInformation($"reasonTwo : {reasonTwo}");
            log.LogInformation($"pageUrl : {pageUrl}");
            log.LogInformation($"startDate : {startDate}");
            log.LogInformation($"endDate : {endDate}");
            log.LogInformation($"sendReportTo : {sendReportTo}");
            log.LogInformation($"isOngoing : {isOngoing}");

            var content = new MultipartFormDataContent
            {
                { new StringContent(UserEmail), "\"email\"" },
                { new StringContent(reasonOne), "\"subject\"" },
                { new StringContent("2"), "\"status\"" },
                { new StringContent("1"), "\"priority\"" },
                { new StringContent(reasonOne), "\"custom_fields[cf_reason]\"" }
,            };

            if (description != null)
            {
                content.Add(new StringContent(description), "\"description\"");
            }

            if (reasonTwo != null && reasonTwo != "")
            {
                content.Add(new StringContent(reasonTwo), "\"custom_fields[cf_reason_2]\"");
            }

            if (pageUrl != null)
            {
                content.Add(new StringContent(pageUrl), "\"custom_fields[cf_page_url]\"");
            }

            if (startDate != null)
            {
                content.Add(new StringContent(startDate), "\"custom_fields[cf_start_date]\"");
            }

            if (endDate != null)
            {
                content.Add(new StringContent(endDate), "\"custom_fields[cf_end_date]\"");
            }

            if (sendReportTo != null)
            {
                content.Add(new StringContent(sendReportTo), "\"custom_fields[cf_send_report_to]\"");
            }

            if (isOngoing != "false" && isOngoing != "")
            {
                content.Add(new StringContent(isOngoing), "\"custom_fields[cf_ongoing]\"");
            }

            if (attachmentData.Count > 0)
            {
                log.LogInformation("Attaching file(s)...");
                foreach (KeyValuePair<string, byte[]> files in attachmentData)
                {
                    content.Add(new ByteArrayContent(files.Value, 0, files.Value.Length), "attachments[]", attachmentName[files.Key]);
                }
            }
            else
            {
                log.LogInformation("No files to attach");
            }

            request.Content = content;

            try
            {
                log.LogInformation("Sending request...");
                var response = await sharedClient.SendAsync(request);
                log.LogInformation($"response.StatusCode: {response.StatusCode}");
                var responseContent = response.Content.ReadAsStringAsync();
                log.LogInformation($"responseContent.Result: {responseContent.Result}");

                return response;
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
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("ERROR");
                log.LogInformation(ex.Message);
                log.LogInformation(ex.StackTrace);
                return null;
            }
        }
    }
}