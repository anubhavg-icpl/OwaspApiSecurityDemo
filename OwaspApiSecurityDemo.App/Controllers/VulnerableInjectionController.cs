using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/injection")]
    public sealed class VulnerableInjectionController : ApiController
    {
        // Anubhav's patch note:
        // The search term is treated like trusted SQL, so payloads like 1 OR 1=1--
        // change the meaning of the query. Switch to SecureInjectionController.SqlUsers()
        // to show the parameterized version immediately after this.
        [HttpGet]
        [Route("sql-users")]
        public IHttpActionResult SqlUsers(string search)
        {
            string query;
            var users = DemoStore.VulnerableSqlSearch(search, out query);

            return Ok(new
            {
                executedQuery = query,
                users,
                warning = "String concatenation allows SQL injection patterns like 1 OR 1=1--"
            });
        }

        [HttpPost]
        [Route("nosql-login")]
        public async Task<IHttpActionResult> NoSqlLogin()
        {
            // Anubhav's patch note:
            // This raw body path lets JSON operators such as $gt act like code instead of plain data.
            var rawBody = await Request.Content.ReadAsStringAsync();
            var user = DemoStore.VulnerableNoSqlLogin(rawBody);

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    error = "Login failed.",
                    note = "Try a body such as {\"username\":{\"$gt\":\"\"},\"password\":{\"$gt\":\"\"}}"
                });
            }

            return Ok(new
            {
                loggedInAs = DemoStore.ToPublicUser(user),
                warning = "Operator injection bypassed credential checks."
            });
        }

        [HttpGet]
        [Route("command-preview")]
        public IHttpActionResult CommandPreview(string fileName)
        {
            // Anubhav's patch note:
            // The demo never executes the command, but it shows exactly how concatenation turns
            // a harmless file name parameter into a shell payload.
            var command = "convert " + (fileName ?? string.Empty) + " -resize 50% output.png";

            return Ok(new
            {
                command,
                dangerous = !string.IsNullOrWhiteSpace(fileName) && Regex.IsMatch(fileName, "[;&|]"),
                warning = "This demo never executes the command; it only shows how unsafe concatenation creates command injection risk."
            });
        }
    }
}
