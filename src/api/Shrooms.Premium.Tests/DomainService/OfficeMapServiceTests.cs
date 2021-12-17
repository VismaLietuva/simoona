using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class OfficeMapServiceTests
    {
        private IOfficeMapService _officeMapService;
        private DbSet<ApplicationUser> _usersDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            var mapper = Substitute.For<IMapper>();
            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);

            _officeMapService = new OfficeMapService(mapper, unitOfWork, uow, roleService);
        }

        private static void MockRoleService(IRoleService roleService)
        {
            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);
            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);
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
                    Room = new Room
                    {
                        FloorId = 1,
                        Floor = new Floor
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
                    Room = new Room
                    {
                        FloorId = 1,
                        Floor = new Floor
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
                    Room = new Room
                    {
                        FloorId = 1,
                        Floor = new Floor
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
                    Room = new Room
                    {
                        FloorId = 2,
                        Floor = new Floor
                        {
                            OfficeId = 3
                        }
                    }
                }
            }.AsQueryable();
        }

        [Test]
        public async Task Should_Return_If_Get_Emails_By_Office_Returns_Incorrect_Emails()
        {
            var emails = await _officeMapService.GetEmailsByOfficeAsync(3);
            Assert.AreEqual(1, emails.Count());
        }

        [Test]
        public async Task Should_Return_If_Get_Emails_By_Floor_Returns_Incorrect_Emails()
        {
            var emails = await _officeMapService.GetEmailsByFloorAsync(1);
            Assert.AreEqual(3, emails.Count());
        }

        [Test]
        public async Task Should_Return_If_Get_Emails_By_Room_Returns_Incorrect_Emails()
        {
            var emails = await _officeMapService.GetEmailsByRoomAsync(1);
            Assert.AreEqual(2, emails.Count());
        }
    }
}
