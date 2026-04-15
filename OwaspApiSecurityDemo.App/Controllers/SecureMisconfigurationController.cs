using System;
using System.Net;
using System.Web.Http;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/secure/misconfig")]
    public sealed class SecureMisconfigurationController : ApiController
    {
        // Runnable patch for the verbose error demo:
        // return a safe client message plus an error ID that can be mapped to server logs.
        [HttpGet]
        [Route("error")]
        public IHttpActionResult Error()
        {
            return Content(HttpStatusCode.InternalServerError, new
            {
                error = "An internal error occurred.",
                errorId = "ERR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                support = "security-demo@local"
            });
        }

        [HttpGet]
        [Route("headers")]
        public IHttpActionResult Headers()
        {
            // Runnable patch for the missing-header demo.
            // The actual headers are injected globally in DemoSecurityHeadersHandler to avoid per-endpoint drift.
            return Ok(new
            {
                example = "Security headers are added centrally for all /api/secure routes.",
                observation = "Open the response headers to show HSTS, CSP, DENY framing, nosniff, and no-store."
            });
        }

        [HttpGet]
        [Route("default-credentials")]
        public IHttpActionResult DefaultCredentials()
        {
            // Runnable patch summary for the default-credentials demo.
            return Ok(new
            {
                policy = "Default credentials are prohibited.",
                practice = "Use unique secrets from a vault or deployment-specific configuration."
            });
        }
    }
}
