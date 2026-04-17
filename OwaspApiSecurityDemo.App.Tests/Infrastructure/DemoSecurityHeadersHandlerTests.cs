using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Tests.Infrastructure
{
    [TestClass]
    public sealed class DemoSecurityHeadersHandlerTests
    {
        [TestMethod]
        public async Task SendAsync_AddsSecureHeadersForSecureRoutes()
        {
            var handler = new DemoSecurityHeadersHandler
            {
                InnerHandler = new StubHandler()
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/secure/misconfig/headers");

            var response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("DENY", string.Join(",", response.Headers.GetValues("X-Frame-Options")));
            Assert.AreEqual("nosniff", string.Join(",", response.Headers.GetValues("X-Content-Type-Options")));
            Assert.AreEqual("no-store", string.Join(",", response.Headers.GetValues("Cache-Control")));
            StringAssert.Contains(string.Join(",", response.Headers.GetValues("Content-Security-Policy")), "default-src 'self'");
        }

        [TestMethod]
        public async Task SendAsync_AddsDebugHeadersForVulnerableRoutes()
        {
            var handler = new DemoSecurityHeadersHandler
            {
                InnerHandler = new StubHandler()
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/vulnerable/misconfig/headers");

            var response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.AreEqual("true", string.Join(",", response.Headers.GetValues("X-Debug-Mode")));
            Assert.AreEqual("OWASP-Demo-Sample", string.Join(",", response.Headers.GetValues("X-Powered-By")));
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request
                });
            }
        }
    }
}
