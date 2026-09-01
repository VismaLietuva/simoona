using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PollEntityConfigTests
    {
        private ShroomsDbContext _dbContext;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ShroomsDbContext(options);

            _dbContext.Set<Organization>().Add(new Organization
            {
                Id = 1,
                ShortName = "Test",
                Name = "TestOrg",
                WelcomeEmail = "Welcome"
            });
            _dbContext.SaveChanges(false);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        // Regression: PollService.DeleteAsync soft deletes the poll and then removes its wall.
        // With DeleteBehavior.Restrict on Poll.Wall, EF threw "the association between entity types
        // 'Wall' and 'Poll' has been severed" on Remove, because the soft deleted poll is still
        // tracked with a non-nullable WallId.
        [Test]
        public async Task Should_Soft_Delete_Poll_And_Its_Wall_Without_Severing_The_Wall_Relationship()
        {
            var wall = new Wall
            {
                Name = "Poll wall",
                OrganizationId = 1,
                Type = WallType.Polls,
                Access = WallAccess.Private
            };

            _dbContext.Set<Wall>().Add(wall);
            await _dbContext.SaveChangesAsync("user");

            var poll = new Poll
            {
                OrganizationId = 1,
                Title = "Title",
                Deadline = DateTime.UtcNow.AddDays(1),
                State = PollState.Draft,
                WallId = wall.Id,
                CreatedBy = "user",
                Questions = new List<PollQuestion>
                {
                    new PollQuestion
                    {
                        Text = "Question",
                        Order = 0,
                        Options = new List<PollOption>
                        {
                            new PollOption { Text = "A", Order = 0 },
                            new PollOption { Text = "B", Order = 1 }
                        }
                    }
                }
            };

            _dbContext.Set<Poll>().Add(poll);
            await _dbContext.SaveChangesAsync(false);

            // PollService.DeleteAsync
            _dbContext.Set<Poll>().Remove(poll);
            await _dbContext.SaveChangesAsync("user");

            // WallService.DeleteWallAsync
            var trackedWall = await _dbContext.Set<Wall>()
                .Include(entity => entity.Moderators)
                .Include(entity => entity.Members)
                .Include(entity => entity.Posts).ThenInclude(post => post.Comments)
                .FirstOrDefaultAsync(entity => entity.Id == poll.WallId &&
                                               entity.OrganizationId == 1 &&
                                               entity.Type == WallType.Polls);

            Assert.That(trackedWall, Is.Not.Null);

            _dbContext.Set<Wall>().Remove(trackedWall);
            await _dbContext.SaveChangesAsync("user");

            Assert.Multiple(() =>
            {
                Assert.That(poll.IsDeleted, Is.True);
                Assert.That(trackedWall.IsDeleted, Is.True);
            });

            Assert.That(await _dbContext.Set<Poll>().CountAsync(), Is.Zero);
            Assert.That(await _dbContext.Set<Wall>().CountAsync(), Is.Zero);
        }
    }
}
