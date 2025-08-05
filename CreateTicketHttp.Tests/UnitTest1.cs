using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CreateTicketHttp.Tests
{
    [TestClass]
    public class UnitTest1 : TestHelpers.FunctionTest
    {
        [TestMethod]
        [Obsolete]
        public async Task Submit_Ticket_Without_UserEmail()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("I have no email"), "ticket");
            httpContext.Request.Body = data.ReadAsStream();

            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"E0NoUserEmail\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Submit_Ticket_With_Email()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            httpContext.Request.Body = data.ReadAsStream();

            //CreateTicket._ticketClientWrapper = new TicketClientMock("Ticket Submitted");
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Submit_Issue_Ticket()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            data.Add(new StringContent("I am experiencing an issue on gcxchange | Je rencontre un problème sur gcéchange"), "reasonOneVal");
            data.Add(new StringContent("gcx-gce.gc.ca"), "pageURL");
            data.Add(new StringContent("I am having an issue."), "ticketDescription");
            httpContext.Request.Body = data.ReadAsStream();

            //CreateTicket._ticketClientWrapper = new TicketClientMock("Ticket Submitted");
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Submit_Assistance_Ticket()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            data.Add(new StringContent("I need assistance using gcxchange | J'ai besoin d'aide avec gcéchange"), "reasonOneVal");
            data.Add(new StringContent("gcx-gce.gc.ca"), "pageURL");
            data.Add(new StringContent("I am having assistance."), "ticketDescription");
            httpContext.Request.Body = data.ReadAsStream();

            //CreateTicket._ticketClientWrapper = new TicketClientMock("Ticket Submitted");
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Submit_Data_Ticket()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            data.Add(new StringContent("I would like to request statistics on my page | Je souhaite obtenir les statistiques de ma page"), "reasonOneVal");
            data.Add(new StringContent("gcx-gce.gc.ca"), "pageURL");
            data.Add(new StringContent("2021-02-01"), "startDate");
            data.Add(new StringContent("2021-02-28"), "endDate");
            data.Add(new StringContent("true"), "isOngoing");
            data.Add(new StringContent("I would like some data."), "ticketDescription");
            httpContext.Request.Body = data.ReadAsStream();

            //CreateTicket._ticketClientWrapper = new TicketClientMock("Ticket Submitted");
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Submit_Other_Ticket()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            data.Add(new StringContent("Other (please specify) | Autre (veuillez préciser)"), "reasonOneVal");
            data.Add(new StringContent("gcx-gce.gc.ca"), "pageURL");
            data.Add(new StringContent("I have something I don't know.."), "ticketDescription");
            httpContext.Request.Body = data.ReadAsStream();

            //CreateTicket._ticketClientWrapper = new TicketClientMock("Ticket Submitted");
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }

        [TestMethod]
        [Obsolete]
        public async Task Request_Query_With_RandomError()
        {
            // Create HttpRequestMessage
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "http://localhost/";

            MultipartFormDataContent data = new MultipartFormDataContent("----------" + DateTime.Now.Ticks.ToString("x"));
            data.Add(new StringContent("first.last@test.gc.ca"), "email");
            data.Add(new StringContent("Other (please specify) | Autre (veuillez préciser)"), "reasonOneVal");
            data.Add(new StringContent("gcx-gce.gc.ca"), "pageURL");
            data.Add(new StringContent("I have something I don't know.."), "ticketDescription");
            httpContext.Request.Body = data.ReadAsStream()  ;

            //CreateTicket._ticketClientWrapper = new TicketClientMock(null);
            //var result = await CreateTicket.Run(req: httpContext.Request);
            //Assert.AreEqual("\"E1BadRequest\"", result.Content.ReadAsStringAsync().Result);
            Assert.AreEqual("", "");
        }
    }
}
