using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CreateTicketHttp.Tests
{
    [TestClass]
    public class UnitTest1 : TestHelpers.FunctionTest
    {
       
        [TestMethod]
        public async Task Request_Query_Without_UserEmail()
        {
            // Create HttpRequestMessage
            var data = "{\"ticket\": { } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            var result = await CreateTicket.Run(req: request, log: log);
            Assert.AreEqual("\"E0NoUserEmail\"", result.Content.ReadAsStringAsync().Result);
        }
    }
}
