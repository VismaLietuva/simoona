using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Exceptions.Exceptions.KudosBaskets;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Domain.ServiceValidators.Validators.KudosBaskets;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    public class KudosBasketServiceTests
    {
        private IDbSet<KudosLog> _kudosLogsDbSet;
        private IDbSet<KudosBasket> _kudosBasketDbSet;
        private KudosBasketService _kudosBasketService;
        private KudosBasketValidator _kudosBasketValidator;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<KudosType> _kudosTypesDbSet;
        private IKudosService _kudosService;

        [SetUp]
        public void TestInitializer()
        {
            _kudosLogsDbSet = Substitute.For<IDbSet<KudosLog>>();
            _kudosBasketDbSet = Substitute.For<IDbSet<KudosBasket>>();
            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _kudosTypesDbSet = Substitute.For<IDbSet<KudosType>>();
            _usersDbSet.SetDbSetData(MockDonator());
            _kudosBasketDbSet.SetDbSetData(MockKudosBaskets());
            _kudosTypesDbSet.SetDbSetData(MockKudosTypes());

            var uow = Substitute.For<IUnitOfWork2>();
            uow.GetDbSet<KudosBasket>().Returns(_kudosBasketDbSet);
            uow.GetDbSet<KudosLog>().Returns(_kudosLogsDbSet);
            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<KudosType>().Returns(_kudosTypesDbSet);

            _kudosBasketValidator = new KudosBasketValidator();
            var mockValidator = Substitute.For<IKudosBasketValidator>();
            _kudosService = Substitute.For<IKudosService>();
            _kudosBasketService = new KudosBasketService(uow, mockValidator, _kudosService);
        }

        private IQueryable<KudosType> MockKudosTypes()
        {
            var types = new List<KudosType>
            {
                new KudosType
                {
                    Id = 1,
                    Name = "Other",
                    Type = KudosTypeEnum.Other
                },

                new KudosType
                {
                    Id = 2,
                    Name = "Minus",
                    Type = KudosTypeEnum.Minus
                }
            }.AsQueryable();
            return types;
        }

        private IQueryable<KudosBasket> MockKudosBaskets()
        {
            var kudosBaskets = new List<KudosBasket>
            {
                new KudosBasket
                {
                    Id = 10,
                    Description = "test",
                    KudosLogs = MockBasketKudosLogs(),
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = true
                },
                new KudosBasket
                {
                    Id = 11,
                    Description = "test",
                    KudosLogs = new List<KudosLog> { new KudosLog() { Points = 5, Employee = MockDonator().First() } },
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = false,
                }
            };
            return kudosBaskets.AsQueryable();
        }

        private ICollection<KudosLog> MockBasketKudosLogs()
        {
            var kudosBasketLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    Points = 10,
                    Created = DateTime.Parse("2015-11-01"),
                    Employee = MockDonator().First()
                },
                new KudosLog
                {
                    Points = 15,
                    Created = DateTime.Parse("2015-11-02"),
                    Employee = MockDonator().First()
                }
            };
            return kudosBasketLogs;
        }

        private IQueryable<ApplicationUser> MockDonator()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "testUserId",
                    FirstName = "Testas",
                    LastName = "Testauskas",
                    RemainingKudos = 100,
                    SpentKudos = 0
                }
            }.AsQueryable();
            return users;
        }

        [Test]
        public void Should_Return_Only_One_Active_Baskets_Donations()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _kudosBasketService.GetDonations(userAndOrg);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Should_Map_Basket_Logs_Correctly()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _kudosBasketService.GetDonations(userAndOrg);
            Assert.AreEqual(15, result.First().DonationAmount);
            Assert.AreEqual(DateTime.Parse("2015-11-02"), result.First().DonationDate);
        }

        [Test]
        public void Should_Return_Basket_Donations_With_Correctly_Mapped_Donator()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _kudosBasketService.GetDonations(userAndOrg);
            Assert.AreEqual("testUserId", result.First().Donator.Id);
            Assert.AreEqual("Testas Testauskas", result.First().Donator.FullName);
        }

        [Test]
        public void Should_Return_Basket_Donations_With_Correctly_Mapped_Deleted_Donator()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            _kudosBasketDbSet.SetDbSetData(new List<KudosBasket>()
            {
                new KudosBasket
                {
                    Id = 11,
                    Description = "test",
                    KudosLogs = new List<KudosLog> { new KudosLog() { Points = 5, Employee = null } },
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = false,
                }
            });

            var result = _kudosBasketService.GetDonations(userAndOrg);
            Assert.AreEqual("Deleted Account", result.First().Donator.FullName);
        }

        [Test]
        public void Should_Throw_Kudos_Basket_Exception_If_Basket_Is_Inactive_During_Donation()
        {
            var basket = new KudosBasket
            {
                IsActive = false
            };
            Assert.Throws<KudosBasketException>(() => _kudosBasketValidator.CheckIfBasketIsActive(basket));
        }

        [Test]
        public void Should_Not_Throw_Kudos_Basket_Exception_If_Basket_Is_Active_During_Donation()
        {
            var basket = new KudosBasket
            {
                IsActive = true
            };
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfBasketIsActive(basket));
        }

        [Test]
        public void Should_Throw_Exception_If_Basket_Exists()
        {
            var basketExists = true;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Assert.Throws<Exception>(() => _kudosBasketValidator.CheckIfBasketAlreadyExists(basketExists));
        }

        [Test]
        public void Should_Not_Throw_Exception_If_There_Are_No_Baskets()
        {
            var basketExists = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfBasketAlreadyExists(basketExists));
        }

        [Test]
        public void Should_Create_New_Basket()
        {
            var newBasket = new KudosBasketCreateDTO
            {
                Description = "test",
                Title = "test",
                UserId = "testUserId",
                OrganizationId = 2
            };
            _kudosBasketService.CreateNewBasket(newBasket);
            _kudosBasketDbSet.Received(1).Add(Arg.Any<KudosBasket>());
        }

        [Test]
        public void Should_Return_Existing_Basket()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            // only one active and not deleted basket can be in a database
            _kudosBasketDbSet.Remove(_kudosBasketDbSet.Find(11));

            var result = _kudosBasketService.GetKudosBasket(userAndOrg);
            Assert.AreEqual(10, result.Id);
            Assert.AreEqual("test", result.Description);
        }

        [Test]
        public void Should_Return_If_Basket_Widget_Is_Active()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            // leave only one deactivated basket
            _kudosBasketDbSet.Remove(_kudosBasketDbSet.Find(11));
            var activeKudosBasket = _kudosBasketDbSet.First(x => x.Id == 10);
            activeKudosBasket.IsActive = false;

            var result = _kudosBasketService.GetKudosBasketWidget(userAndOrg);
            Assert.AreEqual(null, result);
        }

        [Test]
        public void Should_Throw_Kudos_Basket_Exception_If_There_Are_No_Baskets()
        {
            Assert.Throws<KudosBasketException>(() => _kudosBasketValidator.CheckIfThereIsNoBasketYet(null));
        }

        [Test]
        public void Should_Not_Throw_Kudos_Basket_Exception_If_There_Is_Basket_Created()
        {
            var basket = new KudosBasketDTO();
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfThereIsNoBasketYet(basket));
        }

        [Test]
        public void Should_Remove_Existing_Basket()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "testUserId",
                OrganizationId = 2
            };
            _kudosBasketService.DeleteKudosBasket(userAndOrg);
            _kudosBasketDbSet.Received(1).Remove(Arg.Any<KudosBasket>());
        }

        [Test]
        public void Should_Edit_Existing_Kudos_Basket()
        {
            var kudosBasketDto = new KudosBasketEditDTO
            {
                Id = 10,
                IsActive = false,
                Description = "edited",
                Title = "edited",
                UserId = "testUserId"
            };
            _kudosBasketService.EditKudosBasket(kudosBasketDto);
            var editedBasket = _kudosBasketDbSet.First(basket => basket.Id == 10);
            Assert.AreEqual(kudosBasketDto.Title, editedBasket.Title);
            Assert.AreEqual(kudosBasketDto.Description, editedBasket.Description);
            Assert.AreEqual(kudosBasketDto.IsActive, editedBasket.IsActive);
            Assert.AreEqual("testUserId", editedBasket.ModifiedBy);
        }

        [Test]
        public void Should_Make_A_Donation()
        {
            var donationDto = new KudosBasketDonationDTO
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };
            _kudosBasketService.MakeDonation(donationDto);
            var basket = _kudosBasketDbSet.First(b => b.Id == 10);
            Assert.AreEqual(45, basket.KudosLogs.Sum(l => l.Points));
        }

        [Test]
        public void Should_Create_Two_Kudos_Logs_On_Donation()
        {
            var donationDto = new KudosBasketDonationDTO
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };
            _kudosBasketService.MakeDonation(donationDto);
            _kudosLogsDbSet.Received(1).Add(Arg.Any<KudosLog>());
        }

        [Test]
        public void Should_Recalculate_Donators_Kudos()
        {
            var donationDto = new KudosBasketDonationDTO
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };
            _kudosBasketService.MakeDonation(donationDto);
            var user = _usersDbSet.First(u => u.Id == "testUserId");
            _kudosService.Received(1).UpdateProfileKudos(user, donationDto);
        }

        [Test]
        public void Should_Throw_Kudos_Basket_Exception_If_Donator_Has_Insufficient_Kudos()
        {
            var kudosRemaining = 50;
            var kudosDonated = 60;
            Assert.Throws<KudosBasketException>(() => _kudosBasketValidator.CheckIfUserHasEnoughKudos(kudosRemaining, kudosDonated));
        }

        [Test]
        public void Should_Not_Throw_Kudos_Basket_Exception_If_Donator_Has_Enough_Kudos()
        {
            var kudosRemaining = 50;
            var kudosDonated = 40;
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfUserHasEnoughKudos(kudosRemaining, kudosDonated));
        }
    }
}
