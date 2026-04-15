using System;
using System.Net;
using System.Web.Http;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/misconfig")]
    public sealed class VulnerableMisconfigurationController : ApiController
    {
        [HttpGet]
        [Route("error")]
        public IHttpActionResult Error()
        {
            try
            {
                throw new InvalidOperationException("Database connection failed for demo-user@prod-sql-01.");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    error = ex.GetType().Name,
                    message = ex.Message,
                    trace = ex.ToString(),
                    internalPath = @"C:\inetpub\wwwroot\Api\UserService.cs:142",
                    query = "SELECT * FROM Users WHERE Id = '${userId}'"
                });
            }
        }

        [HttpGet]
        [Route("headers")]
        public IHttpActionResult Headers()
        {
            return Ok(new
            {
                example = "Missing security headers and extra debug metadata.",
                observation = "The response intentionally lacks HSTS, CSP, X-Frame-Options, and no-store."
            });
        }

        [HttpGet]
        [Route("default-credentials")]
        public IHttpActionResult DefaultCredentials()
        {
            return Ok(new
            {
                panel = "admin portal",
                username = "admin",
                password = "admin123",
                problem = "Default credentials are still enabled."
            });
        }
    }
}
