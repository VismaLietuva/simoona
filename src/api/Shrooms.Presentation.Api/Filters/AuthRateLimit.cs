using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>Rate-limiting policy for the anonymous authentication endpoints and its partition key.</summary>
    public static class AuthRateLimit
    {
        public const string PolicyName = "auth";
        public const string ClientIpHeader = "X-Client-Ip";
        public const string ClientIpSecretHeader = "X-Client-Ip-Secret";

        /// <summary>
        /// The browser address forwarded by the web client when it proves itself with the shared secret;
        /// otherwise the TCP peer. A missing or wrong secret never widens trust, it only falls back.
        /// </summary>
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
