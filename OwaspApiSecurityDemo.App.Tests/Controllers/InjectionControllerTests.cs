using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwaspApiSecurityDemo.App.Controllers;
using OwaspApiSecurityDemo.App.Infrastructure;
using OwaspApiSecurityDemo.App.Tests.TestHelpers;

namespace OwaspApiSecurityDemo.App.Tests.Controllers
{
    [TestClass]
    public sealed class InjectionControllerTests
    {
        [TestMethod]
        public async Task SecureCommandPreview_RejectsUnsafeFileName()
        {
            var controller = new SecureInjectionController();
            ApiTestHelper.InitializeController(controller, url: "http://localhost/api/secure/injection/command-preview");

            var response = await ApiTestHelper.ExecuteAsync(controller.CommandPreview("invoice.pdf;whoami"));
            var body = await ApiTestHelper.ReadJsonObjectAsync(response);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("File name contains characters outside the allow-list.", body["error"]);
        }

        [TestMethod]
        public async Task VulnerableNoSqlLogin_AcceptsOperatorInjectionPayload()
        {
            var controller = new VulnerableInjectionController();
            ApiTestHelper.InitializeController(controller, method: "POST", url: "http://localhost/api/vulnerable/injection/nosql-login");
            controller.Request.Content = new System.Net.Http.StringContent("{\"username\":{\"$gt\":\"\"},\"password\":{\"$gt\":\"\"}}");

            var response = await ApiTestHelper.ExecuteAsync(await controller.NoSqlLogin());
            var body = await ApiTestHelper.ReadJsonObjectAsync(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Operator injection bypassed credential checks.", body["warning"]);
        }
    }
}
