using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.BusinessLayer.Services.ShroomsInfoMailingService;
using Shrooms.DataLayer.DAL;
using Shrooms.DomainService.Services.Email;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Infrastructure.SystemClock;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    public class ShroomsDailyMailingServiceTests
    {
        private IShroomsDailyMailingService _dailyMailingService;
        private IEmailService _emailService;
        private IUnitOfWork2 _uow2;
        private IDbSet<ApplicationUser> _applicationUserDbSeb;
        private IDbSet<Post> _postDbSet;
        private IDbSet<Hashtag> _hashtagDbSet;
        private IDbSet<WallUsers> _wallUsersDbSet;
        private ISystemClock _systemClock;

        [SetUp]
        public void TestInitializer()
        {
            _uow2 = Substitute.For<IUnitOfWork2>();

            _applicationUserDbSeb = Substitute.For<IDbSet<ApplicationUser>>();
            _applicationUserDbSeb.SetDbSetData(GetMockedUsers());
            _uow2.GetDbSet<ApplicationUser>().Returns(_applicationUserDbSeb);

            _postDbSet = Substitute.For<IDbSet<Post>>();
            _postDbSet.SetDbSetData(GetMockedPosts());
            _uow2.GetDbSet<Post>().Returns(_postDbSet);

            _hashtagDbSet = Substitute.For<IDbSet<Hashtag>>();
            _hashtagDbSet.SetDbSetData(GetMockedHashtags());
            _uow2.GetDbSet<Hashtag>().Returns(_hashtagDbSet);

            _wallUsersDbSet = Substitute.For<IDbSet<WallUsers>>();
            _wallUsersDbSet.SetDbSetData(GetMockedWallUsers());
            _uow2.GetDbSet<WallUsers>().Returns(_wallUsersDbSet);

            _systemClock = Substitute.For<ISystemClock>();
            _emailService = Substitute.For<IEmailService>();
            
            _dailyMailingService = new ShroomsDailyMailingService(_uow2, _systemClock, _emailService);
        }

        [Test]
        public void Should_Send_Daily_Mail_To_Users()
        {
            //Arrange
            //Act
            _systemClock.UtcNow.Returns(new DateTime(2016, 8, 18, 9, 30, 0));
            _dailyMailingService.SendDailyMails();

            //Assert
            var calls = _emailService.ReceivedCalls();
            Assert.That(calls, Is.Not.Null);
            Assert.That(calls.Count(), Is.EqualTo(2));

            _emailService.Received(1).
                SendEmailMessage("user3@visma.com", 
                "<table cellspacing='0' cellpadding='5' border='1'>" +
                "<tr><td><b><u>(2016-08-17 10:00) FirstName LastName</u></b>:<br/>Nobody reads this anyway! WallId is null</td></tr>" +
                "</table>" +
                "<br/>For more news check our <a href='http://simona:8888/Visma' target='_blank'>Simoona</a>",
                "Simoona wall");
            _emailService.Received(1).SendEmailMessage("user4@visma.com",
                "<table cellspacing='0' cellpadding='5' border='1'>" +
                "<tr><td><b><u>(2016-08-18 00:30) FirstName </u></b>:<br/>This has a hashtag that user wants to see! #TestsRock</td></tr>" +
                "<tr><td><b><u>(2016-08-17 10:00)  LastName</u></b>:<br/>LastName instead of user name #TestsRock</td></tr>" +
                "</table>" +
                "<br/>For more news check our <a href='http://simona:8888/Visma' target='_blank'>Simoona</a>",
                "Simoona wall");
        }

        #region Mocks
        private IQueryable<Post> GetMockedPosts()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Created = new DateTime(2016, 08, 18, 10, 0, 0),
                    WallId = 1,
                    MessageBody = "Nobody cares about this post!",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 18, 9, 30, 0),
                    WallId = 1,
                    MessageBody = "This has a hashtag! #YOLO",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 18, 0, 30, 0),
                    WallId = 3,
                    MessageBody = "This has a hashtag that user wants to see! #TestsRock",
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "FirstName"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 17, 10, 0, 0),
                    WallId = 1,
                    MessageBody = "LastName instead of user name #TestsRock",
                    ApplicationUser = new ApplicationUser
                    {
                        LastName = "LastName"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 17, 10, 0, 0),
                    WallId = 2,
                    MessageBody = "No hashtags found in this post.",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 17, 10, 0, 0),
                    WallId = 3,
                    MessageBody = "This has no hashtag also",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 17, 10, 0, 0),
                    WallId = null,
                    MessageBody = "Nobody reads this anyway! WallId is null",
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "FirstName",
                        LastName = "LastName"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 17, 9, 30, 0),
                    WallId = 1,
                    MessageBody = "I can write any peom I want",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                },
                new Post
                {
                    Created = new DateTime(2016, 08, 16, 9, 59, 59),
                    WallId = 1,
                    MessageBody = "Spongebob squarepants",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "The Creator"
                    }
                }
            };
            return posts.AsQueryable();
        }

        private IQueryable<ApplicationUser> GetMockedUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "82F7D6F9-DB3A-4CA4-9FAD-B03224D44D8B",
                    Email = "user1@visma.com",
                    ShroomsInfoMailingHour = null,
                    Organization = new Organization { ShortName = "Visma"}
                },
                new ApplicationUser
                {
                    Id = "F66DA01C-70DD-4ECF-9CF7-A62FD3BCC10A",
                    Email = "user2@visma.com",
                    ShroomsInfoMailingHour = new TimeSpan(10, 0, 0),
                    Organization = new Organization { ShortName = "Visma"}
                },
                new ApplicationUser
                {
                    Id = "07CFAA0B-7A70-44E1-9B83-1FF1B965E9FA",
                    Email = "user3@visma.com",
                    ShroomsInfoMailingHour = new TimeSpan(9, 0, 0),
                    Organization = new Organization { ShortName = "Visma"}
                },
                new ApplicationUser
                {
                    Id = "D03975F7-1E09-40FF-8C83-67494B85B1F4",
                    Email = "user4@visma.com",
                    ShroomsInfoMailingHour = new TimeSpan(9, 0, 0),
                    DisabledHashtags = new List<Hashtag>
                    {
                        new Hashtag { Text = "#NoHashTags" },
                        new Hashtag { Text = "#YOLO" },
                    },
                    Organization = new Organization { ShortName = "Visma"}
                }
            };
            return users.AsQueryable();
        }

        private IQueryable<WallUsers> GetMockedWallUsers()
        {
            var wallUsers = new List<WallUsers>
            {
                new WallUsers
                {
                    UserId = "82F7D6F9-DB3A-4CA4-9FAD-B03224D44D8B",
                    WallId = 1
                },
                new WallUsers
                {
                    UserId = "F66DA01C-70DD-4ECF-9CF7-A62FD3BCC10A",
                    WallId = 1
                },
                new WallUsers
                {
                    UserId = "07CFAA0B-7A70-44E1-9B83-1FF1B965E9FA",
                    WallId = 5
                },
                new WallUsers
                {
                    UserId = "D03975F7-1E09-40FF-8C83-67494B85B1F4",
                    WallId = 1
                },
                new WallUsers
                {
                    UserId = "D03975F7-1E09-40FF-8C83-67494B85B1F4",
                    WallId = 3
                }
            };
            return wallUsers.AsQueryable();
        }

        private IQueryable<Hashtag> GetMockedHashtags()
        {
            var hashtags = new List<Hashtag>
            {
                new Hashtag {Text = "#NoHashTags", Modified = new DateTime(2016, 8, 18, 7, 0, 0)},
                new Hashtag {Text = "#YOLO", Modified = new DateTime(2016, 8, 18, 7, 0, 0)},
                new Hashtag {Text = "#TestsRock", Modified = new DateTime(2016, 8, 18, 7, 0, 0)},
                new Hashtag {Text = "#NoTestsSucks", Modified = new DateTime(2016, 8, 17, 9, 29, 59)}
            };
            return hashtags.AsQueryable();
        }
        #endregion

    }

}
