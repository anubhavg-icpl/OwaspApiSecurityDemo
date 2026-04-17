using System.Web.Http;

namespace OwaspApiSecurityDemo.App.Controllers
{
    public sealed class HomeController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Index()
        {
            return Ok(new
            {
                message = "OWASP API Security presentation demo",
                note = "Use the vulnerable endpoints to show what goes wrong, then repeat with the secure endpoints.",
                browserDemo = "/browser",
                categories = new[]
                {
                    "A05 Security Misconfiguration",
                    "A02 Identification and Authentication Failures",
                    "A03 Injection",
                    "A01 Broken Access Control"
                }
            });
        }
    }
}
