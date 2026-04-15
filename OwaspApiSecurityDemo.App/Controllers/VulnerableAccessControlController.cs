using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/access")]
    public sealed class VulnerableAccessControlController : ApiController
    {
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
            return Ok(new
            {
                exportedUsers = DemoStore.GetPublicUsers(),
                warning = "Admin export is accessible without checking the caller's role."
            });
        }
    }
}
