using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.Groups;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Groups;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class GroupKudosWebHookServiceTests
    {
        private const string OrganizationName = "visma";

        private IGroupKudosWebHookService _service;
        private IGroupKudosService _groupKudosService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            var organizationsDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new Organization { Id = 1, ShortName = OrganizationName }
            });
            uow.GetDbSet<Organization>().Returns(organizationsDbSet);

            _groupKudosService = Substitute.For<IGroupKudosService>();
            _groupKudosService
                .AwardOutstandingMonthsAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<GroupMonthlyKudosResultDto>());
            _groupKudosService
                .AwardMonthlyKudosAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new GroupMonthlyKudosResultDto());

            _service = new GroupKudosWebHookService(
                uow,
                _groupKudosService,
                Substitute.For<ILogger<GroupKudosWebHookService>>());
        }

        [Test]
        public async Task Should_Award_The_Given_Month_When_Both_Halves_Of_The_Period_Are_Given()
        {
            await _service.AwardMonthlyGroupKudosAsync(OrganizationName, 2026, 8);

            await _groupKudosService.Received(1).AwardMonthlyKudosAsync(
                Arg.Is<UserAndOrganizationDto>(u => u.OrganizationId == 1), 2026, 8);
        }

        [Test]
        public async Task Should_Catch_Up_From_Last_Month_When_No_Period_Is_Given()
        {
            var previousMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);

            await _service.AwardMonthlyGroupKudosAsync(OrganizationName);

            await _groupKudosService.Received(1).AwardOutstandingMonthsAsync(
                Arg.Any<UserAndOrganizationDto>(), previousMonth.Year, previousMonth.Month);
        }

        [TestCase(2026, null)]
        [TestCase(null, 8)]
        public void Should_Reject_Half_A_Period_Rather_Than_Silently_Catching_Up(int? year, int? month)
        {
            // Half a period used to fall through to the catch-up, awarding periods the
            // caller never asked for and ignoring the one it did.
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.AwardMonthlyGroupKudosAsync(OrganizationName, year, month));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupInvalidKudosPeriod));
        }
    }
}
