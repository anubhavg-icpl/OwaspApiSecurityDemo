using System;
using System.Net;
using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;

namespace OwaspApiSecurityDemo.App.Controllers
{
    [RoutePrefix("api/secure/auth")]
    public sealed class SecureAuthenticationController : ApiController
    {
        // Runnable patch for the vulnerable login flow:
        // credentials move into the request body, login attempts are constrained,
        // and the response issues a short-lived signed token.
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Username and password are required in the request body."
                });
            }

            DemoUser user;
            string message;
            if (!DemoStore.TryAuthenticateSecure(request.Username, request.Password, DateTime.UtcNow, out user, out message))
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    error = message
                });
            }

            return Ok(new
            {
                token = DemoTokenService.CreateSecureToken(user, TimeSpan.FromMinutes(15)),
                expiresInMinutes = 15,
                protections = new[]
                {
                    "Credentials sent in JSON body instead of query string",
                    "HS256 signature enforced",
                    "Short token lifetime",
                    "Brute-force protection with temporary lockout"
                }
            });
        }

        [HttpPost]
        [Route("validate")]
        public IHttpActionResult Validate(TokenValidationRequest request)
        {
            // Runnable patch for token validation:
            // fail closed unless the token passes signature, issuer, audience, and expiry checks.
            TokenPrincipal principal;
            string error;

            if (!DemoTokenService.TryValidateSecureToken(request != null ? request.Token : null, out principal, out error))
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
                    principal.Role,
                    principal.ExpiresUtc
                }
            });
        }

        [HttpGet]
        [Route("me")]
        public IHttpActionResult Me()
        {
            // Runnable patch for downstream API authorization:
            // every protected endpoint resolves the caller from a validated bearer token.
            DemoUser user;
            string error;
            if (!DemoAuthContext.TryGetSecureUser(Request, out user, out error))
            {
                return Content(HttpStatusCode.Unauthorized, new { error });
            }

            return Ok(new
            {
                profile = DemoStore.ToPublicUser(user),
                note = "This route expects an Authorization: Bearer <token> header."
            });
        }
    }
}
