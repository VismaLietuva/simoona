using System;
using System.Collections.Generic;
using System.Data.Entity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Vacations;
using Shrooms.EntityModels.Models;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService.VacationService
{
    [TestFixture]
    public class VacationServiceTests
    {
        private IVacationService _vacationService;
        private IVacationDomainService _vacationDomainService;
        private IDbSet<ApplicationUser> _applicationUserDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _applicationUserDbSet = uow.MockDbSet<ApplicationUser>();

            _vacationDomainService = new VacationDomainService();
            _vacationService = new Domain.Services.Vacations.VacationService(uow, _vacationDomainService);
        }

        [Test]
        public async void Should_Get_Available_Days()
        {
            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "user1"
            };

            var user = new ApplicationUser()
            {
                Id = "user1",
                VacationUnusedTime = 31,
                VacationLastTimeUpdated = new DateTime(2018, 2, 2)
            };

            var userList = new List<ApplicationUser>()
            {
                user
            };

            _applicationUserDbSet.SetDbSetDataForAsync(userList);

            var response = await _vacationService.GetAvailableDays(userAndOrg);

            Assert.AreEqual(response.AvailableDays, 31);
            Assert.AreEqual(response.LastTimeUpdated, new DateTime(2018, 2, 2));
        }
    }
}
