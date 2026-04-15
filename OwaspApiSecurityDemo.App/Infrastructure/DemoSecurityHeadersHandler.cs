using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OwaspApiSecurityDemo.App.Infrastructure
{
    public sealed class DemoSecurityHeadersHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var path = request.RequestUri.AbsolutePath.ToLowerInvariant();

            if (path.StartsWith("/api/secure"))
            {
                response.Headers.TryAddWithoutValidation("X-Frame-Options", "DENY");
                response.Headers.TryAddWithoutValidation("X-Content-Type-Options", "nosniff");
                response.Headers.TryAddWithoutValidation("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                response.Headers.TryAddWithoutValidation("Content-Security-Policy", "default-src 'self'");
                response.Headers.TryAddWithoutValidation("Cache-Control", "no-store");
            }
            else if (path.StartsWith("/api/vulnerable"))
            {
                response.Headers.TryAddWithoutValidation("X-Debug-Mode", "true");
                response.Headers.TryAddWithoutValidation("X-Powered-By", "OWASP-Demo-Sample");
            }

            return response;
        }
    }
}
