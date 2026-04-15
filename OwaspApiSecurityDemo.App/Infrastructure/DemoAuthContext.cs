using System;
using System.Net.Http;

namespace OwaspApiSecurityDemo.App.Infrastructure
{
    public static class DemoAuthContext
    {
        public static bool TryGetSecureUser(HttpRequestMessage request, out DemoUser user, out string error)
        {
            user = null;
            error = null;

            var token = GetBearerToken(request);
            if (string.IsNullOrWhiteSpace(token))
            {
                error = "Missing bearer token.";
                return false;
            }

            TokenPrincipal principal;
            if (!DemoTokenService.TryValidateSecureToken(token, out principal, out error))
            {
                return false;
            }

            user = DemoStore.FindUserByUserName(principal.Subject);
            if (user == null)
            {
                error = "Token subject does not map to a known user.";
                return false;
            }

            return true;
        }

        public static string GetBearerToken(HttpRequestMessage request)
        {
            if (request == null || request.Headers.Authorization == null)
            {
                return null;
            }

            return string.Equals(request.Headers.Authorization.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
                ? request.Headers.Authorization.Parameter
                : null;
        }
    }
}
