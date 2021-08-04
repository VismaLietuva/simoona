using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PostServiceTests
    {
        private DbSet<Post> _postsDbSet;
        private DbSet<Wall> _wallsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<WallModerator> _wallModeratorsDbSet;
        private IPostService _postService;
        private IPermissionService _permissionService;

        private readonly string _userId = Guid.NewGuid().ToString();

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _postsDbSet = uow.MockDbSetForAsync<Post>();
            _wallsDbSet = uow.MockDbSetForAsync<Wall>();
            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _wallModeratorsDbSet = uow.MockDbSetForAsync<WallModerator>();

            _permissionService = Substitute.For<IPermissionService>();
            var commentService = Substitute.For<ICommentService>();
            _postService = new PostService(uow, _permissionService, commentService);
        }

        [Test]
        public async Task Should_Like_Post()
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

            _postsDbSet.SetDbSetDataForAsync(new List<Post> { post }.AsQueryable());
            await _postService.ToggleLikeAsync(1, new UserAndOrganizationDto { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual("user1", _postsDbSet.First().Likes.First().UserId);
        }

        [Test]
        public async Task Should_Unlike_Post()
        {
            var post = new Post
            {
                Id = 1,
                Likes = new LikesCollection { new Like("user1") },
                Wall = new Wall
                {
                    OrganizationId = 2
                }
            };

            _postsDbSet.SetDbSetDataForAsync(new List<Post> { post }.AsQueryable());
            await _postService.ToggleLikeAsync(1, new UserAndOrganizationDto { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual(0, _postsDbSet.First().Likes.Count);
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Post_To_Be_Liked()
        {
            _postsDbSet.SetDbSetDataForAsync(new List<Post>().AsQueryable());
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _postService.ToggleLikeAsync(1, new UserAndOrganizationDto { UserId = "user1", OrganizationId = 2 }));
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
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // ReSharper disable once CollectionNeverUpdated.Local
            var posts = new List<Post>();
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = _userId }
            };
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());

            var newPostDto = new NewPostDto
            {
                MessageBody = "test",
                OrganizationId = 2,
                PictureId = "pic",
                UserId = _userId,
                WallId = walls.First().Id
            };

            // Act
            _postService.CreateNewPostAsync(newPostDto);

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
            // ReSharper disable once CollectionNeverUpdated.Local
            var walls = new List<Wall>();
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // ReSharper disable once CollectionNeverUpdated.Local
            var posts = new List<Post>();
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "testUser" }
            };
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());

            var newPostDto = new NewPostDto
            {
                MessageBody = "test",
                OrganizationId = 2,
                PictureId = "pic",
                UserId = "testUser",
                WallId = 1
            };

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _postService.CreateNewPostAsync(newPostDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Post_To_Be_Hidden()
        {
            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user1",
                OrganizationId = 2
            };

            _postsDbSet.SetDbSetDataForAsync(new List<Post>().AsQueryable());
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _postService.HideWallPostAsync(1, userOrg));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Not_Authorized_To_Hide_Wall_Post()
        {
            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user1",
                OrganizationId = 2
            };

            var wall = new Wall { Id = 1, OrganizationId = 2 };

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };

            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());
            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(false);

            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user2", IsHidden = false },
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user2", IsHidden = false }
            };

            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1" },
                new ApplicationUser { Id = "user2" }
            };

            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());

            Assert.ThrowsAsync<UnauthorizedException>(async () => await _postService.HideWallPostAsync(1, userOrg));
        }

        [Test]
        public async Task Should_Hide_Wall_Post()
        {
            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user1",
                OrganizationId = 2
            };

            var wall = new Wall { Id = 1, OrganizationId = 2 };

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user1" }
            };

            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());
            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(true);

            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", IsHidden = false },
                new Post { Id = 1, Wall = wall, MessageBody = "post", AuthorId = "user1", IsHidden = false }
            };

            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1" }
            };
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());

            await _postService.HideWallPostAsync(1, userOrg);

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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editPostDto = new EditPostDto
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editPostDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _postService.EditPostAsync(editPostDto));
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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editPostDto = new EditPostDto
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editPostDto, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _postService.EditPostAsync(editPostDto));
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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editPostDto = new EditPostDto
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editPostDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _postService.EditPostAsync(editPostDto));
        }

        [Test]
        public void Should_Throw_If_Post_To_Be_Edited_Does_Not_Exist()
        {
            // Setup
            // ReSharper disable once CollectionNeverUpdated.Local
            var posts = new List<Post>();
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var editPostDto = new EditPostDto
            {
                Id = 1,
                MessageBody = "edited post",
                UserId = "user3",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _postService.EditPostAsync(editPostDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Post_To_Be_Deleted_Does_Not_Exist()
        {
            // Setup
            // ReSharper disable once CollectionNeverUpdated.Local
            var posts = new List<Post>();
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user1",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _postService.DeleteWallPostAsync(1, userOrg));
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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _postService.DeleteWallPostAsync(1, userOrg));
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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _postService.DeleteWallPostAsync(1, userOrg));
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
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _postService.DeleteWallPostAsync(1, userOrg));
        }
    }
}