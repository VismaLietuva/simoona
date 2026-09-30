using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class ReturnUrlValidatorTests
    {
        private ReturnUrlValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new ReturnUrlValidator(new[] { "https://app.simoona.com/", "http://localhost:3000" });
        }

        [TestCase("https://app.simoona.com/en/login/callback")]
        [TestCase("https://APP.simoona.com/lt/login/callback?x=1")]
        [TestCase("http://localhost:3000/en/login/callback")]
        public void IsAllowed_AcceptsConfiguredOrigins(string url)
        {
            Assert.That(_validator.IsAllowed(url), Is.True);
        }

        [TestCase("https://evil.example/steal")]
        [TestCase("https://app.simoona.com.evil.example/")]
        [TestCase("https://app.simoona.com@evil.example/")]
        [TestCase("http://app.simoona.com/en/login/callback")]
        [TestCase("https://app.simoona.com:8443/")]
        [TestCase("http://localhost:3001/")]
        [TestCase("javascript:alert(1)")]
        [TestCase("//evil.example/x")]
        [TestCase("/en/login/callback")]
        [TestCase("https://app.simoona.com/callback#already-has-fragment")]
        [TestCase("https://user:pw@app.simoona.com/")]
        [TestCase("")]
        [TestCase(null)]
        public void IsAllowed_RejectsEverythingElse(string url)
        {
            Assert.That(_validator.IsAllowed(url), Is.False);
        }

        [Test]
        public void IsAllowed_RejectsOverlongUrls()
        {
            var url = "https://app.simoona.com/" + new string('a', 2100);

            Assert.That(_validator.IsAllowed(url), Is.False);
        }

        [Test]
        public void Constructor_CollectsClientUrlAndCorsOrigins_AndIgnoresWildcard()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                ["ClientUrl"] = "https://app.simoona.com/",
                ["CorsOrigins"] = "https://app.simoona.com; https://simoona-app-web.azurewebsites.net",
                ["AllowedReturnUrlOrigins"] = "https://extra.example"
            }).Build();

            var validator = new ReturnUrlValidator(configuration);

            Assert.That(validator.IsAllowed("https://simoona-app-web.azurewebsites.net/en/login/callback"), Is.True);
            Assert.That(validator.IsAllowed("https://extra.example/cb"), Is.True);
            Assert.That(validator.IsAllowed("https://other.example/cb"), Is.False);
        }

        [Test]
        public void Constructor_WildcardCors_OnlyAllowsClientUrl()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                ["ClientUrl"] = "http://localhost:3000/",
                ["CorsOrigins"] = "*"
            }).Build();

            var validator = new ReturnUrlValidator(configuration);

            Assert.That(validator.IsAllowed("http://localhost:3000/en/login/callback"), Is.True);
            Assert.That(validator.IsAllowed("https://evil.example/"), Is.False);
        }
    }
}
