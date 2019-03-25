using System;
using System.Collections.Generic;
using System.Data.Entity;
using DomainServiceValidators.Validators.UserAdministration;
using Microsoft.AspNet.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Email.AdministrationUsers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Picture;
using Shrooms.DomainExceptions.Exceptions.UserAdministration;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.UnitTests;
using Shrooms.UnitTests.Extensions;
using Shrooms.UnitTests.ModelMappings;

namespace Shrooms.API.Tests.DomainService
{
    public class AdministrationUsersServiceTests
    {
        private IUserAdministrationValidator _userAdministrationValidator;
        private IAdministrationUsersService _administrationUsersService;
        private ShroomsUserManager _userManager;
        private IOrganizationService _organizationService;
        private IPictureService _pictureService;
        private IAdministrationNotificationService _administrationUsersNotificationService;
        private IDbSet<ApplicationUser> _userDbSet;
        private IDbSet<Wall> _wallsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork>();
            var uow2 = Substitute.For<IUnitOfWork2>();

            var dbContext = Substitute.For<IDbContext>();
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = MockIdentity.MockUserManager(userStore, dbContext);
         
            _organizationService = Substitute.For<IOrganizationService>();
            _pictureService = Substitute.For<IPictureService>();
            _administrationUsersNotificationService = Substitute.For<IAdministrationNotificationService>();

            _userDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            uow2.GetDbSet<ApplicationUser>().Returns(_userDbSet);

            _wallsDbSet = Substitute.For<IDbSet<Wall>>();
            uow2.GetDbSet<Wall>().Returns(_wallsDbSet);

            _userAdministrationValidator = new UserAdministrationValidator();
            _administrationUsersService = new AdministrationUsersService(ModelMapper.Create(), uow, uow2, _userAdministrationValidator, _userManager, _organizationService, _pictureService, dbContext, _administrationUsersNotificationService);
        }

        [Test]
        public void Should_Throw_Exception_If_Users_Employment_Date_Is_Not_Set()
        {
            Assert.Throws<UserAdministrationException>(() => _userAdministrationValidator.CheckIfEmploymentDateIsSet(null));
        }

        [Test]
        public void Should_Not_Throw_Exception_If_Users_Employment_Date_Is_Set()
        {
            Assert.DoesNotThrow(() => _userAdministrationValidator.CheckIfEmploymentDateIsSet(DateTime.UtcNow));
        }

        [Test]
        public void Should_Throw_Exception_If_User_Has_Not_Filled_His_Info_Yet()
        {
            Assert.Throws<UserAdministrationException>(() => _userAdministrationValidator.CheckIfUserHasFirstLoginRole(true));
        }

        [Test]
        public void Should_Not_Throw_Exception_If_User_Has_Filled_His_Info()
        {
            Assert.Throws<UserAdministrationException>(() => _userAdministrationValidator.CheckIfUserHasFirstLoginRole(true));
        }

        [Test]
        public void Should_Throw_Exception_If_User_Has_Filled_His_Info()
        {
            Assert.DoesNotThrow(() => _userAdministrationValidator.CheckIfUserHasFirstLoginRole(false));
        }

        [Test]
        public void Should_Throw_Exception_If_Adding_Removing_Role_While_Confirming_User_Returns_Errors()
        {
            Assert.DoesNotThrow(() => _userAdministrationValidator.CheckForAddingRemovingRoleErrors(new List<string>(), new List<string>()));
        }

        [Test]
        public void Should_Not_Throw_Exception_If_Adding_Removing_Role_While_Confirming_User_Returns_Errors()
        {
            var addRoleErrors = new List<string> { "error1", "error2" };
            var removeRoleErrors = new List<string> { "error1", "error2" };
            Assert.Throws<UserAdministrationException>(() => _userAdministrationValidator.CheckForAddingRemovingRoleErrors(addRoleErrors, removeRoleErrors));
        }
        
        [Test]
        public void Should_Set_User_Tutorial_Status_To_Completed()
        {
            var users = new List<ApplicationUser>()
            {
                new ApplicationUser() { Id = "user1", EmploymentDate = new DateTime(2018, 5, 15) }
            };

            _userDbSet.SetDbSetData(users);

            _administrationUsersService.SetUserTutorialStatusToComplete("user1");

            Assert.IsTrue(users[0].IsTutorialComplete);
        }
    }
}
