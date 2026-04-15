using System.Net;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/secure/access")]
    public sealed class SecureAccessControlController : ApiController
    {
        // Runnable patch for BOLA:
        // resolve the caller from a validated token and reject cross-user access unless the caller is an admin.
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
            // Runnable patch for the admin export:
            // fail closed unless the authenticated caller has the admin role.
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
