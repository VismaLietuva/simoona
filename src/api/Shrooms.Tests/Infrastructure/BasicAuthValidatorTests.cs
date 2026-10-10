using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.BasicAuth;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class BasicAuthValidatorTests
    {
        private const string Tenant = "visma";
        private const string ConfiguredUser = "job-runner";
        private const string ConfiguredPassword = "correct horse battery staple";

        private IApplicationSettings _settings;
        private BasicAuthValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _settings = Substitute.For<IApplicationSettings>();
            _settings.BasicUsername.Returns(ConfiguredUser);
            _settings.BasicPassword.Returns(ConfiguredPassword);

            var organizations = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            organizations.SetDbSetDataForAsync(new List<Organization> { new() { Id = 1, ShortName = Tenant } });

            var dbContext = Substitute.For<IDbContext>();
            dbContext.Set<Organization>().Returns(organizations);

            _validator = new BasicAuthValidator(_settings, dbContext);
        }

        [Test]
        public void Validate_CorrectCredentialsAndKnownTenant_ReturnsPrincipalWithTenantClaim()
        {
            var principal = _validator.Validate(ConfiguredUser, ConfiguredPassword, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.IsAuthenticated, Is.True);

            var claims = ((System.Security.Claims.ClaimsPrincipal)principal).Claims.ToList();
            Assert.That(claims.Single(c => c.Type == WebApiConstants.ClaimOrganizationName).Value, Is.EqualTo(Tenant));
            Assert.That(claims.Single(c => c.Type == "role").Value, Is.EqualTo("scheduler-webhook"));
        }

        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("", ConfiguredPassword)]
        [TestCase(ConfiguredUser, "")]
        public void Validate_EmptySuppliedCredentials_ReturnsNull(string userName, string password)
        {
            var principal = _validator.Validate(userName, password, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Null);
        }

        [Test]
        public void Validate_UnconfiguredCredentials_RejectsEmptyHeaderInsteadOfFailingOpen()
        {
            _settings.BasicUsername.Returns(string.Empty);
            _settings.BasicPassword.Returns(string.Empty);

            var principal = _validator.Validate(string.Empty, string.Empty, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Null);
        }

        [TestCase(" ", " ")]
        [TestCase("\t", "\t")]
        public void Validate_WhitespaceOnlyConfiguration_NeverMatchesWhitespaceCredentials(string configured, string supplied)
        {
            _settings.BasicUsername.Returns(configured);
            _settings.BasicPassword.Returns(configured);

            var principal = _validator.Validate(supplied, supplied, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Null);
        }

        [Test]
        public void Validate_UnconfiguredPasswordOnly_ReturnsNull()
        {
            _settings.BasicPassword.Returns(string.Empty);

            var principal = _validator.Validate(ConfiguredUser, string.Empty, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Null);
        }

        [TestCase("job-runner", "wrong")]
        [TestCase("wrong", ConfiguredPassword)]
        [TestCase("JOB-RUNNER", ConfiguredPassword)]
        [TestCase("job-runner", "correct horse battery staple ")]
        public void Validate_MismatchedCredentials_ReturnsNull(string userName, string password)
        {
            var principal = _validator.Validate(userName, password, CancellationToken.None, ContextFor(Tenant));

            Assert.That(principal, Is.Null);
        }

        [TestCase("unknown")]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_UnknownOrMissingTenant_ReturnsNull(string tenant)
        {
            var principal = _validator.Validate(ConfiguredUser, ConfiguredPassword, CancellationToken.None, ContextFor(tenant));

            Assert.That(principal, Is.Null);
        }

        private static HttpContext ContextFor(string tenant)
        {
            var context = new DefaultHttpContext();
            if (tenant != null)
            {
                context.Items["tenantName"] = tenant;
            }

            return context;
        }
    }
}
