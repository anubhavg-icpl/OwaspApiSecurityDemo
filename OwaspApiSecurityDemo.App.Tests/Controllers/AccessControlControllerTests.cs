using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwaspApiSecurityDemo.App.Controllers;
using OwaspApiSecurityDemo.App.Infrastructure;
using OwaspApiSecurityDemo.App.Tests.TestHelpers;

namespace OwaspApiSecurityDemo.App.Tests.Controllers
{
    [TestClass]
    public sealed class AccessControlControllerTests
    {
        [TestMethod]
        public async Task SecureOrders_ReturnsForbiddenForCrossUserAccess()
        {
            var controller = new SecureAccessControlController();
            ApiTestHelper.InitializeController(controller, url: "http://localhost/api/secure/access/orders/2");
            ApiTestHelper.AddBearerToken(
                controller,
                DemoTokenService.CreateSecureToken(DemoStore.FindUserByUserName("alice"), System.TimeSpan.FromMinutes(15)));

            var response = await ApiTestHelper.ExecuteAsync(controller.Orders(2));
            var body = await ApiTestHelper.ReadJsonObjectAsync(response);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("You can only access your own orders unless you are an administrator.", body["error"]);
        }

        [TestMethod]
        public async Task SecureAdminExport_ReturnsForbiddenForNonAdmin()
        {
            var controller = new SecureAccessControlController();
            ApiTestHelper.InitializeController(controller, url: "http://localhost/api/secure/access/admin/export");
            ApiTestHelper.AddBearerToken(
                controller,
                DemoTokenService.CreateSecureToken(DemoStore.FindUserByUserName("alice"), System.TimeSpan.FromMinutes(15)));

            var response = await ApiTestHelper.ExecuteAsync(controller.AdminExport());
            var body = await ApiTestHelper.ReadJsonObjectAsync(response);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("Administrator role required.", body["error"]);
        }

        [TestMethod]
        public async Task VulnerableOrders_ReturnsOtherUsersOrdersWithoutAuthorization()
        {
            var controller = new VulnerableAccessControlController();
            ApiTestHelper.InitializeController(controller, url: "http://localhost/api/vulnerable/access/orders/2");

            var response = await ApiTestHelper.ExecuteAsync(controller.Orders(2));
            var body = await ApiTestHelper.ReadJsonObjectAsync(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("not checked", body["caller"]);
            Assert.AreEqual(1, ((System.Collections.ArrayList)body["orders"]).Count);
        }
    }
}
