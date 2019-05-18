using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization;
using Shrooms.Domain.Services.Roles;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.OfficeMap;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.Premium.UnitTests.DomainService
{
    [TestFixture]
    public class OfficeMapServiceTests
    {
        private IOfficeMapService _officeMapService;
        private IDbSet<ApplicationUser> _usersDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _usersDbSet.SetDbSetData(MockUsers());

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            var mapper = Substitute.For<IMapper>();
            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);

            _officeMapService = new OfficeMapService(mapper, unitOfWork, uow, roleService);
        }

        private static void MockRoleService(IRoleService roleService)
        {
            roleService.ExcludeUsersWithRole(Roles.NewUser).Returns(x => true);
        }

        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "testUserId",
                    RoomId = 1,
                    Email = "email1",
                    Room = new Room()
                    {
                        FloorId = 1,
                        Floor = new Floor()
                        {
                            OfficeId = 1
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "testUserId2",
                    RoomId = 1,
                    Email = "email2",
                    Room = new Room()
                    {
                        FloorId = 1,
                        Floor = new Floor()
                        {
                            OfficeId = 1
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "testUserId3",
                    RoomId = 2,
                    Email = "email3",
                    Room = new Room()
                    {
                        FloorId = 1,
                        Floor = new Floor()
                        {
                            OfficeId = 1
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "testUserId4",
                    RoomId = 2,
                    Email = "email4",
                    Room = new Room()
                    {
                        FloorId = 2,
                        Floor = new Floor()
                        {
                            OfficeId = 3
                        }
                    }
                }
            }.AsQueryable();
        }

        [Test]
        public void Should_Return_If_Get_Emails_By_Office_Returns_Incorrect_Emails()
        {
            var emails = _officeMapService.GetEmailsByOffice(3);
            Assert.AreEqual(1, emails.Count());
        }

        [Test]
        public void Should_Return_If_Get_Emails_By_Floor_Returns_Incorrect_Emails()
        {
            var emails = _officeMapService.GetEmailsByFloor(1);
            Assert.AreEqual(3, emails.Count());
        }

        [Test]
        public void Should_Return_If_Get_Emails_By_Room_Returns_Incorrect_Emails()
        {
            var emails = _officeMapService.GetEmailsByRoom(1);
            Assert.AreEqual(2, emails.Count());
        }
    }
}
