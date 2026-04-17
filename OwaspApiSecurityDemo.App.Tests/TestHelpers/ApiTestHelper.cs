using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OwaspApiSecurityDemo.App.Tests.TestHelpers
{
    internal static class ApiTestHelper
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static void InitializeController(ApiController controller, string method = "GET", string url = "http://localhost/")
        {
            controller.Request = new HttpRequestMessage(new HttpMethod(method), url);
            controller.Configuration = new HttpConfiguration();
        }

        public static void AddBearerToken(ApiController controller, string token)
        {
            controller.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<HttpResponseMessage> ExecuteAsync(IHttpActionResult result)
        {
            return await result.ExecuteAsync(CancellationToken.None);
        }

        public static async Task<Dictionary<string, object>> ReadJsonObjectAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Serializer.Deserialize<Dictionary<string, object>>(json);
        }

        public static OkNegotiatedContentResult<T> AssertOk<T>(IHttpActionResult result)
        {
            var ok = result as OkNegotiatedContentResult<T>;
            AssertResult(ok != null, "Expected an OkNegotiatedContentResult.");
            return ok;
        }

        public static NegotiatedContentResult<T> AssertNegotiated<T>(IHttpActionResult result, System.Net.HttpStatusCode statusCode)
        {
            var negotiated = result as NegotiatedContentResult<T>;
            AssertResult(negotiated != null, "Expected a NegotiatedContentResult.");
            AssertResult(negotiated.StatusCode == statusCode, "Unexpected status code.");
            return negotiated;
        }

        private static void AssertResult(bool condition, string message)
        {
            if (!condition)
            {
                Assert.Fail(message);
            }
        }
    }
}
