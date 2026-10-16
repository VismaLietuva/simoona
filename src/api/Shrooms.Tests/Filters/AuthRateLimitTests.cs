using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Tests.Filters
{
    [TestFixture]
    public class AuthRateLimitTests
    {
        private const string Secret = "shared-secret-value";

        private static HttpContext Context(string peer, string clientIp = null, string secret = null)
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(peer);
            if (clientIp != null)
            {
                context.Request.Headers[AuthRateLimit.ClientIpHeader] = clientIp;
            }

            if (secret != null)
            {
                context.Request.Headers[AuthRateLimit.ClientIpSecretHeader] = secret;
            }

            return context;
        }

        [Test]
        public void UsesForwardedClientAddress_WhenTheSharedSecretMatches()
        {
            var key = AuthRateLimit.PartitionKey(Context("10.0.0.5", "203.0.113.7", Secret), Secret);

            Assert.That(key, Is.EqualTo("client:203.0.113.7"));
        }

        [TestCase(null, null)]
        [TestCase("203.0.113.7", null)]
        [TestCase("203.0.113.7", "wrong-secret")]
        [TestCase("not-an-ip", Secret)]
        public void FallsBackToThePeer_WhenTheHeaderCannotBeTrusted(string clientIp, string secret)
        {
            var key = AuthRateLimit.PartitionKey(Context("10.0.0.5", clientIp, secret), Secret);

            Assert.That(key, Is.EqualTo("peer:10.0.0.5"));
        }

        [Test]
        public void IgnoresTheHeader_WhenNoSecretIsConfigured()
        {
            var key = AuthRateLimit.PartitionKey(Context("10.0.0.5", "203.0.113.7", "anything"), null);

            Assert.That(key, Is.EqualTo("peer:10.0.0.5"));
        }
    }
}
