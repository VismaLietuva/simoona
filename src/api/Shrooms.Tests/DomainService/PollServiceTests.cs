using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Models.Polls;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Polls;
using Shrooms.Domain.Services.Polls;
using Shrooms.Domain.Services.Wall;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PollServiceTests
    {
        private const string Voter = "voter";

        private ShroomsDbContext _dbContext;
        private IWallService _wallService;
        private PollService _pollService;

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

            _wallService = Substitute.For<IWallService>();
            _pollService = new PollService(new UnitOfWork2(_dbContext), _wallService);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Should_Persist_The_Wall_Membership_Staged_While_Voting()
        {
            var poll = await AddPollAsync(isAnonymous: false);

            // The real WallService only stages the row, it never saves. VoteAsync has to run its own
            // save afterwards, otherwise the membership dies with the context.
            _wallService
                .AddMemberToWallsAsync(Voter, Arg.Any<List<int>>())
                .Returns(Task.CompletedTask)
                .AndDoes(_ => _dbContext.Set<WallMember>().Add(new WallMember
                {
                    UserId = Voter,
                    WallId = poll.WallId,
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true
                }));

            await _pollService.VoteAsync(BuildVote(poll));

            var member = await _dbContext.Set<WallMember>()
                .FirstOrDefaultAsync(entity => entity.WallId == poll.WallId && entity.UserId == Voter);

            Assert.That(member, Is.Not.Null);
            Assert.That(member.Created, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task Should_Not_Join_The_Wall_When_The_Poll_Is_Anonymous()
        {
            var poll = await AddPollAsync(isAnonymous: true);

            await _pollService.VoteAsync(BuildVote(poll));

            await _wallService
                .DidNotReceive()
                .AddMemberToWallsAsync(Arg.Any<string>(), Arg.Any<List<int>>());

            var answers = await _dbContext.Set<PollAnswer>().Where(a => a.PollId == poll.Id).ToListAsync();
            Assert.That(answers, Is.Not.Empty);
            Assert.That(answers.All(answer => answer.ApplicationUserId == null), Is.True);
        }

        [Test]
        public void Should_Reject_A_Second_Vote_From_The_Same_User()
        {
            var poll = AddPollAsync(isAnonymous: false).GetAwaiter().GetResult();

            _pollService.VoteAsync(BuildVote(poll)).GetAwaiter().GetResult();

            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pollService.VoteAsync(BuildVote(poll)));

            Assert.That(exception.Message, Is.EqualTo("You have already voted in this poll."));
        }

        [Test]
        public async Task Should_Rename_The_Wall_When_The_Poll_Is_Renamed()
        {
            var poll = await AddPollAsync(isAnonymous: false);

            await _pollService.UpdateAsync(new UpdatePollDto
            {
                Id = poll.Id,
                UserId = "author",
                OrganizationId = 1,
                Title = "New title",
                Description = "New description",
                Deadline = poll.Deadline,
                Questions = new List<CreatePollQuestionDto>
                {
                    new CreatePollQuestionDto
                    {
                        Text = "Question",
                        Options = new List<CreatePollOptionDto>
                        {
                            new CreatePollOptionDto { Text = "A" },
                            new CreatePollOptionDto { Text = "B" }
                        }
                    }
                }
            }, canManage: true);

            var wall = await _dbContext.Set<Wall>().FirstAsync(entity => entity.Id == poll.WallId);

            Assert.Multiple(() =>
            {
                Assert.That(wall.Name, Is.EqualTo("New title"));
                Assert.That(wall.Description, Is.EqualTo("New description"));
            });
        }

        private async Task<Poll> AddPollAsync(bool isAnonymous)
        {
            var wall = new Wall
            {
                Name = "Poll wall",
                OrganizationId = 1,
                Type = WallType.Polls,
                Access = WallAccess.Private
            };

            _dbContext.Set<Wall>().Add(wall);
            await _dbContext.SaveChangesAsync("author");

            var poll = new Poll
            {
                OrganizationId = 1,
                Title = "Title",
                IsAnonymous = isAnonymous,
                Deadline = DateTime.UtcNow.AddDays(1),
                State = PollState.Published,
                WallId = wall.Id,
                CreatedBy = "author",
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

            return poll;
        }

        private static PollVoteDto BuildVote(Poll poll)
        {
            var question = poll.Questions.First();

            return new PollVoteDto
            {
                PollId = poll.Id,
                UserId = Voter,
                OrganizationId = 1,
                Answers = new List<PollQuestionAnswerDto>
                {
                    new PollQuestionAnswerDto
                    {
                        QuestionId = question.Id,
                        OptionIds = new List<int> { question.Options.First().Id }
                    }
                }
            };
        }
    }
}
