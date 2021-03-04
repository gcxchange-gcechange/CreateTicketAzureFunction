using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System;
using System.Text;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace CreateTicketHttp
{
    public static class CreateTicket
    {
        [FunctionName("CreateTicket")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // parse query parameter
            string userName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "username", true) == 0)
                .Value;

            string userEmail = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "useremail", true) == 0)
                .Value;

            string option = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "option", true) == 0)
                .Value;

            string userText = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "userText", true) == 0)
                .Value;
           

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            userName = userName ?? data?.ticket.userName;
            userEmail = userEmail ?? data?.ticket.userEmail;
            option = option ?? data?.ticket.option;
            userText = userText ?? data?.ticket.userText;

            // Check if userEmail is passed
            // return BadRequest if not present
            if (userEmail != null)
            {
                var result = CreateTicketAPI(log, userName, userEmail, option, userText);

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

        public static Task<object> CreateTicketAPI(TraceWriter log, string UserName, string UserEmail, string Option, string UserText)
        {

            log.Info(UserName);
            log.Info(UserEmail);
            log.Info(Option);
            log.Info(UserText);

            return Task.FromResult<object>("something");
            
           
        }
    }
}
