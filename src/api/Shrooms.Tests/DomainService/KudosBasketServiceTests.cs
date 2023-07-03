using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
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
        private DbSet<KudosLog> _kudosLogsDbSet;
        private DbSet<KudosBasket> _kudosBasketDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<KudosType> _kudosTypesDbSet;
        private KudosBasketService _kudosBasketService;
        private KudosBasketValidator _kudosBasketValidator;
        private IKudosService _kudosService;

        [SetUp]
        public void TestInitializer()
        {
            _kudosLogsDbSet = Substitute.For<DbSet<KudosLog>, IQueryable<KudosLog>, IDbAsyncEnumerable<KudosLog>>();
            _kudosBasketDbSet = Substitute.For<DbSet<KudosBasket>, IQueryable<KudosBasket>, IDbAsyncEnumerable<KudosBasket>>();
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _kudosTypesDbSet = Substitute.For<DbSet<KudosType>, IQueryable<KudosType>, IDbAsyncEnumerable<KudosType>>();

            _kudosBasketDbSet.SetDbSetDataForAsync(MockKudosBaskets());
            _usersDbSet.SetDbSetDataForAsync(MockDonator());
            _kudosTypesDbSet.SetDbSetDataForAsync(MockKudosTypes());

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

        private static IQueryable<KudosType> MockKudosTypes()
        {
            var types = new List<KudosType>
            {
                new()
                {
                    Id = 1,
                    Name = "Other",
                    Type = KudosTypeEnum.Other
                },

                new()
                {
                    Id = 2,
                    Name = "Minus",
                    Type = KudosTypeEnum.Minus
                }
            }.AsQueryable();
            return types;
        }

        private static IQueryable<KudosBasket> MockKudosBaskets()
        {
            var kudosBaskets = new List<KudosBasket>
            {
                new()
                {
                    Id = 10,
                    Description = "test",
                    KudosLogs = MockBasketKudosLogs(),
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = true
                },
                new()
                {
                    Id = 11,
                    Description = "test",
                    KudosLogs = new List<KudosLog>
                    {
                        new() { Points = 5, Employee = MockDonator().First() }
                    },
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = false
                }
            };
            return kudosBaskets.AsQueryable();
        }

        private static ICollection<KudosLog> MockBasketKudosLogs()
        {
            var kudosBasketLogs = new List<KudosLog>
            {
                new()
                {
                    Points = 10,
                    Created = DateTime.Parse("2015-11-01"),
                    Employee = MockDonator().First()
                },
                new()
                {
                    Points = 15,
                    Created = DateTime.Parse("2015-11-02"),
                    Employee = MockDonator().First()
                }
            };
            return kudosBasketLogs;
        }

        private static IQueryable<ApplicationUser> MockDonator()
        {
            var users = new List<ApplicationUser>
            {
                new()
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
        public async Task Should_Return_Only_One_Active_Baskets_Donations()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = await _kudosBasketService.GetDonationsAsync(userAndOrg);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task Should_Map_Basket_Logs_Correctly()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = await _kudosBasketService.GetDonationsAsync(userAndOrg);
            Assert.AreEqual(15, result.First().DonationAmount);
            Assert.AreEqual(DateTime.Parse("2015-11-02"), result.First().DonationDate);
        }

        [Test]
        public async Task Should_Return_Basket_Donations_With_Correctly_Mapped_Donator()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var result = await _kudosBasketService.GetDonationsAsync(userAndOrg);
            Assert.AreEqual("testUserId", result.First().Donator.Id);
            Assert.AreEqual("Testas Testauskas", result.First().Donator.FullName);
        }

        [Test]
        public async Task Should_Return_Basket_Donations_With_Correctly_Mapped_Deleted_Donator()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            _kudosBasketDbSet.SetDbSetDataForAsync(new List<KudosBasket>
            {
                new()
                {
                    Id = 11,
                    Description = "test",
                    KudosLogs = new List<KudosLog>
                    {
                        new()
                            { Points = 5, Employee = null }
                    },
                    OrganizationId = 2,
                    Title = "test",
                    IsActive = false
                }
            });

            var result = await _kudosBasketService.GetDonationsAsync(userAndOrg);
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
            const bool basketExists = true;

            Assert.Throws<Exception>(() => _kudosBasketValidator.CheckIfBasketAlreadyExists(basketExists));
        }

        [Test]
        public void Should_Not_Throw_Exception_If_There_Are_No_Baskets()
        {
            const bool basketExists = false;

            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfBasketAlreadyExists(basketExists));
        }

        [Test]
        public async Task Should_Create_New_Basket()
        {
            var newBasket = new KudosBasketCreateDto
            {
                Description = "test",
                Title = "test",
                UserId = "testUserId",
                OrganizationId = 2
            };

            await _kudosBasketService.CreateNewBasketAsync(newBasket);
            _kudosBasketDbSet.Received(1).Add(Arg.Any<KudosBasket>());
        }

        [Test]
        public async Task Should_Return_Existing_Basket()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            // only one active and not deleted basket can be in a database
            var kudosBasket = await _kudosBasketDbSet.FirstAsync(x => x.Id == 11);
            _kudosBasketDbSet.Remove(kudosBasket);

            var result = await _kudosBasketService.GetKudosBasketAsync(userAndOrg);
            Assert.AreEqual(10, result.Id);
            Assert.AreEqual("test", result.Description);
        }

        [Test]
        public async Task Should_Return_If_Basket_Widget_Is_Active()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            // leave only one deactivated basket
            var kudosBasket = await _kudosBasketDbSet.FirstAsync(x => x.Id == 11);
            _kudosBasketDbSet.Remove(kudosBasket);

            var activeKudosBasket = await _kudosBasketDbSet.FirstAsync(x => x.Id == 10);
            activeKudosBasket.IsActive = false;

            var result = await _kudosBasketService.GetKudosBasketWidgetAsync(userAndOrg);
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
            var basket = new KudosBasketDto();
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfThereIsNoBasketYet(basket));
        }

        [Test]
        public async Task Should_Remove_Existing_Basket()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = "testUserId",
                OrganizationId = 2
            };

            await _kudosBasketService.DeleteKudosBasketAsync(userAndOrg);
            _kudosBasketDbSet.Received(1).Remove(Arg.Any<KudosBasket>());
        }

        [Test]
        public async Task Should_Edit_Existing_Kudos_Basket()
        {
            var kudosBasketDto = new KudosBasketEditDto
            {
                Id = 10,
                IsActive = false,
                Description = "edited",
                Title = "edited",
                UserId = "testUserId"
            };

            await _kudosBasketService.EditKudosBasketAsync(kudosBasketDto);
            var editedBasket = await _kudosBasketDbSet.FirstAsync(basket => basket.Id == 10);

            Assert.AreEqual(kudosBasketDto.Title, editedBasket.Title);
            Assert.AreEqual(kudosBasketDto.Description, editedBasket.Description);
            Assert.AreEqual(kudosBasketDto.IsActive, editedBasket.IsActive);
            Assert.AreEqual("testUserId", editedBasket.ModifiedBy);
        }

        [Test]
        public async Task Should_Make_A_Donation()
        {
            var donationDto = new KudosBasketDonationDto
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };

            await _kudosBasketService.MakeDonationAsync(donationDto);
            var basket = await _kudosBasketDbSet.FirstAsync(b => b.Id == 10);

            Assert.AreEqual(45, basket.KudosLogs.Sum(l => l.Points));
        }

        [Test]
        public async Task Should_Create_Two_Kudos_Logs_On_Donation()
        {
            var donationDto = new KudosBasketDonationDto
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };

            await _kudosBasketService.MakeDonationAsync(donationDto);
            _kudosLogsDbSet.Received(1).Add(Arg.Any<KudosLog>());
        }

        [Test]
        public async Task Should_Recalculate_Donators_Kudos()
        {
            var donationDto = new KudosBasketDonationDto
            {
                DonationAmount = 20,
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 10
            };

            await _kudosBasketService.MakeDonationAsync(donationDto);

            var user = await _usersDbSet.FirstAsync(u => u.Id == "testUserId");

            await _kudosService.Received(1).UpdateProfileKudosAsync(user, donationDto);
        }

        [Test]
        public void Should_Throw_Kudos_Basket_Exception_If_Donator_Has_Insufficient_Kudos()
        {
            const int kudosRemaining = 50;
            const int kudosDonated = 60;
            Assert.Throws<KudosBasketException>(() => _kudosBasketValidator.CheckIfUserHasEnoughKudos(kudosRemaining, kudosDonated));
        }

        [Test]
        public void Should_Not_Throw_Kudos_Basket_Exception_If_Donator_Has_Enough_Kudos()
        {
            const int kudosRemaining = 50;
            const int kudosDonated = 40;
            Assert.DoesNotThrow(() => _kudosBasketValidator.CheckIfUserHasEnoughKudos(kudosRemaining, kudosDonated));
        }
    }
}
