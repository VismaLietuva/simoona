using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.ServiceValidators.Validators.Wall;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class WallValidatorTests
    {
        private IWallValidator _wallValidator;
        private DbSet<WallMember> _wallUsersDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _wallUsersDbSet = Substitute.For<DbSet<WallMember>, IQueryable<WallMember>, IDbAsyncEnumerable<WallMember>>();
            uow.GetDbSet<WallMember>().Returns(_wallUsersDbSet);

            _wallValidator = new WallValidator(uow);
        }

        [Test]
        public void Should_Not_Throw_When_User_Is_Wall_Member()
        {
            MockWallUsers();

            const string userId = "userId2";
            const int wallId = 1;

            Assert.DoesNotThrow(() => _wallValidator.CheckIfUserIsWallMember(userId, wallId));
        }

        [Test]
        public void Should_Throw_When_User_Is_Not_A_Wall_Member()
        {
            MockWallUsers();

            const string userId = "userId";
            const int wallId = 1;

            Assert.Throws<ValidationException>(() => _wallValidator.CheckIfUserIsWallMember(userId, wallId));
        }

        [Test]
        public void Should_Not_Throw_When_Wall_Id_Is_Not_Present()
        {
            MockWallUsers();

            const string userId = "userId";

            Assert.DoesNotThrow(() => _wallValidator.CheckIfUserIsWallMember(userId, null));
        }

        private void MockWallUsers()
        {
            var wallUsers = new List<WallMember>
            {
                new WallMember
                {
                    WallId = 1,
                    UserId = "userId2"
                }
            }.AsQueryable();

            _wallUsersDbSet.SetDbSetDataForAsync(wallUsers);
        }
    }
}
