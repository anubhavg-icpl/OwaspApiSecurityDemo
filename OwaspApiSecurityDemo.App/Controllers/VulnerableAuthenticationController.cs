using System;
using System.Net;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/auth")]
    public sealed class VulnerableAuthenticationController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(string username, string password)
        {
            DemoUser user;
            if (!DemoStore.ValidateCredentials(username, password, out user))
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    error = "Invalid credentials.",
                    note = "This endpoint still leaks credentials in the URL and does not rate-limit attempts."
                });
            }

            return Ok(new
            {
                token = DemoTokenService.CreateVulnerableToken(user),
                expiresInHours = 24,
                note = "Token uses alg:none semantics and can be tampered with because the signature is not verified."
            });
        }

        [HttpPost]
        [Route("validate")]
        public IHttpActionResult Validate(TokenValidationRequest request)
        {
            TokenPrincipal principal;
            string error;

            if (!DemoTokenService.TryValidateVulnerableToken(request != null ? request.Token : null, out principal, out error))
            {
                return Content(HttpStatusCode.BadRequest, new { error });
            }

            return Ok(new
            {
                accepted = true,
                principal = new
                {
                    principal.UserId,
                    principal.Subject,
                    principal.Role
                },
                warning = "This validator trusts whatever role is inside the token payload."
            });
        }

        [HttpGet]
        [Route("credential-transport")]
        public IHttpActionResult CredentialTransport(string username, string password)
        {
            return Ok(new
            {
                receivedUser = username,
                receivedPassword = password,
                problem = "Credentials arrived in the URL and may be stored in browser history, server logs, and proxy logs."
            });
        }
    }
}
