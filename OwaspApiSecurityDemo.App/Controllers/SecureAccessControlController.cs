using System.Net;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/secure/access")]
    public sealed class SecureAccessControlController : ApiController
    {
        [HttpGet]
        [Route("orders/{userId:int}")]
        public IHttpActionResult Orders(int userId)
        {
            DemoUser caller;
            string error;
            if (!DemoAuthContext.TryGetSecureUser(Request, out caller, out error))
            {
                return Content(HttpStatusCode.Unauthorized, new { error });
            }

            if (caller.Role != "admin" && caller.Id != userId)
            {
                return Content(HttpStatusCode.Forbidden, new
                {
                    error = "You can only access your own orders unless you are an administrator."
                });
            }

            return Ok(new
            {
                caller = DemoStore.ToPublicUser(caller),
                orders = DemoStore.GetOrdersForUser(userId)
            });
        }

        [HttpGet]
        [Route("admin/export")]
        public IHttpActionResult AdminExport()
        {
            DemoUser caller;
            string error;
            if (!DemoAuthContext.TryGetSecureUser(Request, out caller, out error))
            {
                return Content(HttpStatusCode.Unauthorized, new { error });
            }

            if (caller.Role != "admin")
            {
                return Content(HttpStatusCode.Forbidden, new
                {
                    error = "Administrator role required."
                });
            }

            return Ok(new
            {
                caller = DemoStore.ToPublicUser(caller),
                exportedUsers = DemoStore.GetPublicUsers()
            });
        }
    }
}
