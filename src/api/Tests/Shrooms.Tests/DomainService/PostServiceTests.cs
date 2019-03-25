using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class PostServiceTests
    {
        private IDbSet<Post> _postsDbSet;
        private IPostService _postService;
        private IDbSet<Wall> _wallsDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IPermissionService _permissionService;
        private IDbSet<WallModerator> _wallModeratorsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _postsDbSet = uow.MockDbSet<Post>();
            _wallsDbSet = uow.MockDbSet<Wall>();
            _usersDbSet = uow.MockDbSet<ApplicationUser>();
            _wallModeratorsDbSet = uow.MockDbSet<WallModerator>();

            _permissionService = Substitute.For<IPermissionService>();
            var postNotificationService = Substitute.For<IPostNotificationService>();

            var commentService = Substitute.For<ICommentService>();

            _postService = new PostService(uow, _permissionService, postNotificationService, commentService);
        }

        [Test]
        public void Should_Like_Post()
        {
            var post = new Post
            {
                Id = 1,
                Likes = new LikesCollection(),
                Wall = new Wall
                {
                    OrganizationId = 2
                }
            };

            _postsDbSet.SetDbSetData(new List<Post> { post }.AsQueryable());
            _postService.ToggleLike(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual("user1", _postsDbSet.First().Likes.First().UserId);
        }

        [Test]
        public void Should_Unlike_Post()
        {
            var post = new Post
            {
                Id = 1,
                Likes = new LikesCollection() { new Like("user1") },
                Wall = new Wall
                {
                    OrganizationId = 2
                }
            };

            _postsDbSet.SetDbSetData(new List<Post> { post }.AsQueryable());
            _postService.ToggleLike(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual(0, _postsDbSet.First().Likes.Count);
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Post_To_Be_Liked()
        {
            _postsDbSet.SetDbSetData(new List<Post>().AsQueryable());
            var ex = Assert.Throws<ValidationException>(() => _postService.ToggleLike(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 }));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Create_New_Wall_Post()
        {
            // Setup
            var walls = new List<Wall>
            {
                new Wall { Id = 1, OrganizationId = 2 }
            };
            _wallsDbSet.SetDbSetData(walls.AsQueryable());

            var posts = new List<Post>();
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "testUser" }
            };
            _usersDbSet.SetDbSetData(users.AsQueryable());

            var newPostDto = new NewPostDTO
            {
                MessageBody = "test",
                OrganizationId = 2,
                PictureId = "pic",
                UserId = "testUser",
                WallId = walls.First().Id
            };

            // Act
            _postService.CreateNewPost(newPostDto);

            // Assert
            _postsDbSet.Received().Add(
                Arg.Is<Post>(x =>
                    x.MessageBody == newPostDto.MessageBody &&
                    x.WallId == newPostDto.WallId &&
                    x.AuthorId == newPostDto.UserId));
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Wall_To_Add_Posts_To()
        {
            // Setup
            var walls = new List<Wall>();
            _wallsDbSet.SetDbSetData(walls.AsQueryable());

            var posts = new List<Post>();
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "testUser" }
            };
            _usersDbSet.SetDbSetData(users.AsQueryable());

            var newPostDto = new NewPostDTO
            {
                MessageBody = "test",
                OrganizationId = 2,
                PictureId = "pic",
                UserId = "testUser",
                WallId = 1
            };

            // Act
            // Assert
            var ex = Assert.Throws<ValidationException>(() => _postService.CreateNewPost(newPostDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Post_To_Be_Hidden()
        {
            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user1",
                OrganizationId = 2
            };

            _postsDbSet.SetDbSetData(new List<Post>().AsQueryable());
            var ex = Assert.Throws<ValidationException>(() => _postService.HideWallPost(1, userOrg));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Not_Authorized_To_Hide_Wall_Post()
        {
            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user1",
                OrganizationId = 2
            };

            var wall = new Wall { Id = 1, OrganizationId = 2 };

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };

            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());
            _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post).Returns(false);

            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user2", IsHidden = false },
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user2", IsHidden = false }
            };

            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1" },
                new ApplicationUser { Id = "user2" }
            };

            _usersDbSet.SetDbSetData(users.AsQueryable());

            Assert.Throws<UnauthorizedException>(() => _postService.HideWallPost(1, userOrg));
        }

        [Test]
        public void Should_Hide_Wall_Post()
        {
            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user1",
                OrganizationId = 2
            };

            var wall = new Wall { Id = 1, OrganizationId = 2 };

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user1" }
            };

            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());
            _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post).Returns(true);

            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", IsHidden = false },
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", IsHidden = false }
            };

            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1" }
            };
            _usersDbSet.SetDbSetData(users.AsQueryable());

            _postService.HideWallPost(1, userOrg);

            Assert.AreEqual(posts[0].IsHidden, true);
            Assert.AreEqual(posts[1].IsHidden, false);
        }

        [Test]
        public void Should_Not_Throw_If_Moderator_Edits_Other_User_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", WallId = wall.Id }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var editPostDto = new EditPostDTO
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(editPostDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrow(() => _postService.EditPost(editPostDto));
        }

        [Test]
        public void Should_Not_Throw_If_Administrator_Edits_Other_User_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1" }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var editPostDto = new EditPostDTO
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(editPostDto, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrow(() => _postService.EditPost(editPostDto));
        }

        [Test]
        public void Should_Throw_If_User_Edits_Other_User_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1" }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var editPostDto = new EditPostDTO
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(editPostDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.Throws<UnauthorizedException>(() => _postService.EditPost(editPostDto));
        }

        [Test]
        public void Should_Throw_If_Post_To_Be_Edited_Does_Not_Exist()
        {
            // Setup
            var posts = new List<Post>();
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var editPostDto = new EditPostDTO
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.Throws<ValidationException>(() => _postService.EditPost(editPostDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Post_To_Be_Deleted_Does_Not_Exist()
        {
            // Setup
            var posts = new List<Post>();
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user1",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.Throws<ValidationException>(() => _postService.DeleteWallPost(1, userOrg));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_User_Deletes_Other_User_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1" }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.Throws<UnauthorizedException>(() => _postService.DeleteWallPost(1, userOrg));
        }

        [Test]
        public void Should_Not_Throw_If_Administrator_Deletes_Other_User_Wall_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1" }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrow(() => _postService.DeleteWallPost(1, userOrg));
        }

        [Test]
        public void Should_Not_Throw_If_Moderator_Deletes_Other_User_Post()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", WallId = wall.Id }
            };
            _postsDbSet.SetDbSetData(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetData(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrow(() => _postService.DeleteWallPost(1, userOrg));
        }
    }
}