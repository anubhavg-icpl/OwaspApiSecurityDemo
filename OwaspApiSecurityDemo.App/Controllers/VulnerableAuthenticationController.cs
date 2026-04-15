using System;
using System.Net;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/vulnerable/auth")]
    public sealed class VulnerableAuthenticationController : ApiController
    {
        // Presenter patch note:
        // This vulnerable login demonstrates three issues in one place:
        // 1. credentials travel in the URL,
        // 2. there is no lockout or throttling,
        // 3. the returned token is not strongly protected.
        // Walk to SecureAuthenticationController.Login() right after this to show the runnable fix.
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
            // Presenter patch note:
            // This trusts whatever claims arrive in the token payload.
            // The secure validator enforces signature, issuer, audience, and expiration checks.
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
            // Presenter patch note:
            // Use this to explain how URLs leak into browser history, reverse-proxy logs, and monitoring systems.
            return Ok(new
            {
                receivedUser = username,
                receivedPassword = password,
                problem = "Credentials arrived in the URL and may be stored in browser history, server logs, and proxy logs."
            });
        }
    }
}
