using System;
using System.Net;
using System.Web.Http;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/misconfig")]
    public sealed class VulnerableMisconfigurationController : ApiController
    {
        // Presenter patch note:
        // Show this response first to explain why raw exception details should never cross the API boundary.
        // Then switch to SecureMisconfigurationController.Error() to show the runnable patch:
        // generic message to the client, correlation ID for support, and full details kept server-side only.
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
            // Presenter patch note:
            // This endpoint intentionally relies on missing defaults.
            // The patch is centralized in DemoSecurityHeadersHandler so every /api/secure route
            // automatically returns HSTS, CSP, frame protection, content-type protection, and no-store.
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
            // Presenter patch note:
            // This simulates the classic "admin/admin123" mistake.
            // The secure pair documents the fix: unique deployment secrets and secret-manager-backed storage.
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
