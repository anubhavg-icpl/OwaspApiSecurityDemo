using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/secure/injection")]
    public sealed class SecureInjectionController : ApiController
    {
        [HttpGet]
        [Route("sql-users")]
        public IHttpActionResult SqlUsers(int id)
        {
            var user = DemoStore.FindUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                executedQuery = "SELECT Id, UserName, Role FROM Users WHERE Id = @id",
                parameters = new { id },
                user = DemoStore.ToPublicUser(user)
            });
        }

        [HttpPost]
        [Route("nosql-login")]
        public IHttpActionResult NoSqlLogin(LoginRequest request)
        {
            DemoUser user;
            if (request == null || !DemoStore.ValidateCredentials(request.Username, request.Password, out user))
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    error = "Invalid username or password.",
                    note = "Strong typing prevents operator objects such as $gt from entering the credential flow."
                });
            }

            return Ok(new
            {
                loggedInAs = DemoStore.ToPublicUser(user)
            });
        }

        [HttpGet]
        [Route("command-preview")]
        public IHttpActionResult CommandPreview(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !Regex.IsMatch(fileName, "^[A-Za-z0-9_.-]+$"))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "File name contains characters outside the allow-list."
                });
            }

            return Ok(new
            {
                commandTemplate = "convert <safe-file> -resize 50% output.png",
                acceptedFileName = fileName
            });
        }
    }
}
