using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>Rate-limit policy for the anonymous auth endpoints. Product-specific header names: App Service sets its own X-Client-IP.</summary>
    public static class AuthRateLimit
    {
        public const string PolicyName = "auth";
        public const string ClientIpHeader = "X-Simoona-Client-Ip";
        public const string ClientIpSecretHeader = "X-Simoona-Client-Ip-Secret";

        /// <summary>The forwarded browser address when the shared secret matches, otherwise the TCP peer.</summary>
        public static string PartitionKey(HttpContext httpContext, string trustedClientIpSecret)
        {
            if (!string.IsNullOrEmpty(trustedClientIpSecret)
                && httpContext.Request.Headers.TryGetValue(ClientIpSecretHeader, out var suppliedSecret)
                && httpContext.Request.Headers.TryGetValue(ClientIpHeader, out var clientIp)
                && IPAddress.TryParse(clientIp.ToString().Trim(), out var parsed)
                && FixedTimeEquals(suppliedSecret.ToString(), trustedClientIpSecret))
            {
                return "client:" + parsed;
            }

            return "peer:" + (httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        }

        private static bool FixedTimeEquals(string supplied, string expected)
        {
            var a = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
            var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}
