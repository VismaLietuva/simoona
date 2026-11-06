using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class KudosCalculatorSettingsServiceTests
    {
        private const int OtherOrganizationId = 2;

        private IKudosCalculatorSettingsService _service;
        private DbSet<KudosCalculatorSettings> _settingsDbSet;
        private IUnitOfWork2 _uow;
        private List<KudosCalculatorSettings> _settings;

        [SetUp]
        public void TestInitializer()
        {
            _settings = new List<KudosCalculatorSettings>
            {
                new()
                {
                    Id = 1,
                    OrganizationId = OtherOrganizationId,
                    VatPercent = 25,
                    TaxPercent = 50,
                    TeamMultiplier = 2,
                    CreditPercent = 10,
                    TechWearMonths = 24,
                    FurnitureWearMonths = 60,
                    BuyoutMinimum = 5,
                    GuideUrl = "https://example.com/guide"
                }
            };

            _uow = Substitute.For<IUnitOfWork2>();
            _settingsDbSet = _uow.MockDbSetForAsync(_settings);

            _service = new KudosCalculatorSettingsService(_uow);
        }

        [Test]
        public async Task Should_Return_Defaults_When_Organization_Has_No_Settings()
        {
            var result = await _service.GetAsync(UserOrg());

            Assert.That(result.VatPercent, Is.EqualTo(21));
            Assert.That(result.TaxPercent, Is.EqualTo(43));
            Assert.That(result.TeamMultiplier, Is.EqualTo(1.5m));
            Assert.That(result.CreditPercent, Is.EqualTo(20));
            Assert.That(result.TechWearMonths, Is.EqualTo(36));
            Assert.That(result.FurnitureWearMonths, Is.EqualTo(72));
            Assert.That(result.BuyoutMinimum, Is.EqualTo(1));
            Assert.That(result.GuideUrl, Is.Null);
        }

        [Test]
        public async Task Should_Return_Settings_Of_The_Requesting_Organization()
        {
            var result = await _service.GetAsync(UserOrg(OtherOrganizationId));

            Assert.That(result.VatPercent, Is.EqualTo(25));
            Assert.That(result.TechWearMonths, Is.EqualTo(24));
            Assert.That(result.GuideUrl, Is.EqualTo("https://example.com/guide"));
        }

        [Test]
        public async Task Should_Create_Settings_On_First_Update()
        {
            var input = ValidInput();
            input.GuideUrl = "  https://example.com/kudos  ";
            input.WhitelistUrl = " ";

            await _service.UpdateAsync(input, UserOrg());

            _settingsDbSet.Received(1).Add(Arg.Is<KudosCalculatorSettings>(s =>
                s.OrganizationId == TestConstants.DefaultOrganizationId &&
                s.VatPercent == 21 &&
                s.GuideUrl == "https://example.com/kudos" &&
                s.WhitelistUrl == null));
            await _uow.Received(1).SaveChangesAsync("user");
        }

        [Test]
        public async Task Should_Update_Existing_Settings()
        {
            var input = ValidInput();
            input.TaxPercent = 45;

            await _service.UpdateAsync(input, UserOrg(OtherOrganizationId));

            _settingsDbSet.DidNotReceive().Add(Arg.Any<KudosCalculatorSettings>());
            Assert.That(_settings[0].TaxPercent, Is.EqualTo(45));
            Assert.That(_settings[0].GuideUrl, Is.Null);
            await _uow.Received(1).SaveChangesAsync("user");
        }

        [TestCase(-1, 43, 1.5, 36)]
        [TestCase(21, 201, 1.5, 36)]
        [TestCase(21, 43, 0.5, 36)]
        [TestCase(21, 43, 1.5, 0)]
        [TestCase(21.555, 43, 1.5, 36)]
        [TestCase(21, 43, 1.505, 36)]
        public void Should_Reject_Rates_Out_Of_Range(decimal vat, decimal tax, decimal team, int techWear)
        {
            var input = ValidInput();
            input.VatPercent = vat;
            input.TaxPercent = tax;
            input.TeamMultiplier = team;
            input.TechWearMonths = techWear;

            var ex = Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(input, UserOrg()));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.KudosCalculatorSettingsInvalid));
            _settingsDbSet.DidNotReceive().Add(Arg.Any<KudosCalculatorSettings>());
        }

        [Test]
        public async Task Should_Accept_Rates_With_Two_Decimal_Places()
        {
            var input = ValidInput();
            input.VatPercent = 21.55m;
            input.TeamMultiplier = 1.25m;

            await _service.UpdateAsync(input, UserOrg());

            _settingsDbSet.Received(1).Add(Arg.Is<KudosCalculatorSettings>(s => s.VatPercent == 21.55m && s.TeamMultiplier == 1.25m));
        }

        [TestCase("not a url")]
        [TestCase("javascript:alert(1)")]
        [TestCase("ftp://example.com/guide")]
        public void Should_Reject_Links_That_Are_Not_Web_Addresses(string url)
        {
            var input = ValidInput();
            input.WhitelistUrl = url;

            var ex = Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(input, UserOrg()));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.KudosCalculatorSettingsInvalid));
        }

        private static KudosCalculatorSettingsDto ValidInput()
        {
            return KudosCalculatorSettingsService.Defaults;
        }

        private static UserAndOrganizationDto UserOrg()
        {
            return UserOrg(TestConstants.DefaultOrganizationId);
        }

        private static UserAndOrganizationDto UserOrg(int organizationId)
        {
            return new UserAndOrganizationDto { OrganizationId = organizationId, UserId = "user" };
        }
    }
}
