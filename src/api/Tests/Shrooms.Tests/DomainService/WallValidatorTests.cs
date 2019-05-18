using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DomainServiceValidators.Validators.Wall;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.DAL;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService.ValidatorTests
{
    [TestFixture]
    public class WallValidatorTests
    {
        private IWallValidator _wallValidator;
        private IDbSet<WallMember> _wallUsersDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _wallUsersDbSet = Substitute.For<IDbSet<WallMember>>();
            uow.GetDbSet<WallMember>().Returns(_wallUsersDbSet);

            _wallValidator = new WallValidator(uow);
        }

        [Test]
        public void Should_Not_Throw_When_User_Is_Wall_Member()
        {
            MockWallUsers();

            var userId = "userId2";
            var wallId = 1;

            Assert.DoesNotThrow(() => _wallValidator.CheckIfUserIsWallMember(userId, wallId));
        }

        [Test]
        public void Should_Throw_When_User_Is_Not_A_Wall_Member()
        {
            MockWallUsers();

            var userId = "userId";
            var wallId = 1;

            Assert.Throws<ValidationException>(() => _wallValidator.CheckIfUserIsWallMember(userId, wallId));
        }

        [Test]
        public void Should_Not_Throw_When_Wall_Id_Is_Not_Present()
        {
            MockWallUsers();

            var userId = "userId";

            Assert.DoesNotThrow(() => _wallValidator.CheckIfUserIsWallMember(userId, null));
        }

        //[Test]
        //public void Should_Not_Throw_When_User_Is_Posting_To_Subwall()
        //{
        //    var userId = "userId";
        //    var wallId = 2;

        //    MockWallUsersForPostCreate(userId);

        //    Assert.DoesNotThrow(() => _wallValidator.CheckIfUserCanCreatePostInWall(userId, wallId));
        //}

        //[Test]
        //public void Should_Throw_When_User_Is_Posting_To_Main_Wall()
        //{
        //    var userId = "userId";
        //    var wallId = 1;

        //    MockWallUsersForPostCreate(userId);

        //    Assert.Throws<ValidationException>(() => _wallValidator.CheckIfUserCanCreatePostInWall(userId, wallId));
        //}

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

            _wallUsersDbSet.SetDbSetData(wallUsers);
        }

        //private void MockWallUsersForPostCreate(string userId)
        //{
        //    var wallUsers = new List<WallMember>
        //    {
        //        new WallMember
        //        {
        //            WallId = 1,
        //            UserId = userId,
        //            Wall = new Wall
        //            {
        //                ParentId = null
        //            }
        //        },
        //        new WallMember
        //        {
        //            WallId = 2,
        //            UserId = userId,
        //            Wall = new Wall
        //            {
        //                ParentId = 1
        //            }
        //        }
        //    }.AsQueryable();

        //    _wallUsersDbSet.SetDbSetData(wallUsers);
        //}
    }
}
