using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class ExternalEmailTrustTests
    {
        private const string VismaTenant = "4a9b5c2d-1111-2222-3333-444455556666";

        private static ClaimsPrincipal Principal(params (string Type, string Value)[] claims)
        {
            var identity = new ClaimsIdentity("test");
            foreach (var (type, value) in claims)
            {
                identity.AddClaim(new Claim(type, value));
            }

            return new ClaimsPrincipal(identity);
        }

        [Test]
        public void Microsoft_IsTrustedOnlyFromAConfiguredTenant()
        {
            var trust = new ExternalEmailTrust(new[] { VismaTenant });

            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", VismaTenant))), Is.True);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", VismaTenant.ToUpperInvariant()))), Is.True);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", "9188040d-6c67-4c5b-b112-36a304b66dad"))), Is.False, "consumer (MSA) tenant");
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", Guid.NewGuid().ToString()))), Is.False, "attacker's own tenant");
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal((ClaimTypes.Email, "x@visma.com"))), Is.False, "no tenant claim");
        }

        [Test]
        public void Microsoft_IsNeverTrustedWithoutConfiguration()
        {
            Assert.That(new ExternalEmailTrust(null).IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", VismaTenant))), Is.False);
            Assert.That(new ExternalEmailTrust(new[] { " ", "" }).HasTrustedMicrosoftTenants, Is.False);
        }

        [Test]
        public void Google_RequiresEmailVerified_AndFacebookIsTrusted()
        {
            var trust = new ExternalEmailTrust(null);

            Assert.That(trust.IsEmailVerified(AuthenticationConstants.GoogleLoginProvider, Principal(("email_verified", "true"))), Is.True);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.GoogleLoginProvider, Principal(("email_verified", "false"))), Is.False);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.GoogleLoginProvider, Principal()), Is.False);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.FacebookLoginProvider, Principal()), Is.True);
            Assert.That(trust.IsEmailVerified("Unknown", Principal(("tid", VismaTenant))), Is.False);
        }

        [Test]
        public void FromConfiguration_SplitsTheDelimitedList()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new System.Collections.Generic.KeyValuePair<string, string>(ExternalEmailTrust.TrustedTenantsSetting, $" {VismaTenant}; other-tenant ,third") })
                .Build();

            var trust = ExternalEmailTrust.FromConfiguration(configuration);

            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", "other-tenant"))), Is.True);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", "third"))), Is.True);
            Assert.That(trust.IsEmailVerified(AuthenticationConstants.MicrosoftLoginProvider, Principal(("tid", "fourth"))), Is.False);
        }

        [Test]
        public void ReadMicrosoftTenantId_DecodesTheIdTokenPayload()
        {
            static string B64Url(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var idToken = $"{B64Url("{\"alg\":\"RS256\"}")}.{B64Url("{\"aud\":\"client\",\"tid\":\"" + VismaTenant + "\",\"email\":\"x@visma.com\"}")}.signature";
            using var response = JsonDocument.Parse($"{{\"access_token\":\"a\",\"id_token\":\"{idToken}\"}}");

            Assert.That(ExternalEmailTrust.ReadMicrosoftTenantId(response.RootElement), Is.EqualTo(VismaTenant));
        }

        [TestCase("{\"access_token\":\"a\"}")]
        [TestCase("{\"access_token\":\"a\",\"id_token\":\"not-a-jwt\"}")]
        [TestCase("{\"access_token\":\"a\",\"id_token\":\"a.!!!.c\"}")]
        [TestCase("{\"access_token\":\"a\",\"id_token\":\"a.e30.c\"}")]
        public void ReadMicrosoftTenantId_ReturnsNullForMissingOrMalformedTokens(string json)
        {
            using var response = JsonDocument.Parse(json);

            Assert.That(ExternalEmailTrust.ReadMicrosoftTenantId(response.RootElement), Is.Null);
        }
    }
}
