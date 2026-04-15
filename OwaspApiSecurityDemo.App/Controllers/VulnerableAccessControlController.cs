using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/access")]
    public sealed class VulnerableAccessControlController : ApiController
    {
        // Anubhav's patch note:
        // This is the classic BOLA example: the API trusts the path parameter and never checks ownership.
        // Compare it to SecureAccessControlController.Orders() to show the object-level authorization patch.
        [HttpGet]
        [Route("orders/{userId:int}")]
        public IHttpActionResult Orders(int userId)
        {
            return Ok(new
            {
                requestedUserId = userId,
                caller = "not checked",
                orders = DemoStore.GetOrdersForUser(userId),
                warning = "Any caller can enumerate another user's records by changing the path parameter."
            });
        }

        [HttpGet]
        [Route("admin/export")]
        public IHttpActionResult AdminExport()
        {
            // Anubhav's patch note:
            // This is a vertical privilege escalation demo: an admin action is exposed without a role check.
            return Ok(new
            {
                exportedUsers = DemoStore.GetPublicUsers(),
                warning = "Admin export is accessible without checking the caller's role."
            });
        }
    }
}
