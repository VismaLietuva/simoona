using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Polls;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Polls;
using Shrooms.Domain.Services.Wall;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Polls
{
    public class PollService : IPollService
    {
        private const int MinOptionsPerQuestion = 2;
        private const int MaxOptionsPerQuestion = 10;
        private const int MaxQuestionsPerPoll = 10;

        private const string BallotLockedMessage =
            "Voting has started, so the questions and answers can no longer be changed.";

        private readonly IUnitOfWork2 _uow;
        private readonly IWallService _wallService;
        private readonly DbSet<Poll> _pollDbSet;
        private readonly DbSet<PollQuestion> _questionDbSet;
        private readonly DbSet<PollOption> _optionDbSet;
        private readonly DbSet<PollAnswer> _answerDbSet;
        private readonly DbSet<PollParticipant> _participantDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<MultiwallWall> _wallDbSet;

        public PollService(IUnitOfWork2 uow, IWallService wallService)
        {
            _uow = uow;
            _wallService = wallService;
            _pollDbSet = uow.GetDbSet<Poll>();
            _questionDbSet = uow.GetDbSet<PollQuestion>();
            _optionDbSet = uow.GetDbSet<PollOption>();
            _answerDbSet = uow.GetDbSet<PollAnswer>();
            _participantDbSet = uow.GetDbSet<PollParticipant>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _wallDbSet = uow.GetDbSet<MultiwallWall>();
        }

        public async Task<IEnumerable<PollListItemDto>> GetVisiblePollsAsync(UserAndOrganizationDto userOrg)
        {
            var polls = await BaseQuery(userOrg)
                .Where(poll => poll.State == PollState.Published ||
                               ((poll.State == PollState.Pending || poll.State == PollState.Rejected) &&
                                poll.CreatedBy == userOrg.UserId))
                .ToListAsync();

            return await MapListAsync(polls, userOrg);
        }

        public async Task<IEnumerable<PollListItemDto>> GetAllPollsAsync(UserAndOrganizationDto userOrg)
        {
            var polls = await BaseQuery(userOrg).ToListAsync();

            return await MapListAsync(polls, userOrg);
        }

        public async Task<PollDto> GetPollAsync(int id, UserAndOrganizationDto userOrg, bool canManage)
        {
            var poll = await BaseQuery(userOrg)
                .Include(entity => entity.Questions)
                .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(entity => entity.Id == id);

            if (poll == null)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            var isAuthor = poll.CreatedBy == userOrg.UserId;
            if (poll.State != PollState.Published && !canManage && !isAuthor)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            var votedByMe = await HasVotedAsync(poll.Id, userOrg.UserId);
            var canSeeResults = poll.State == PollState.Published && (votedByMe || IsClosed(poll) || canManage);

            var dto = new PollDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                IsAnonymous = poll.IsAnonymous,
                IsOfficial = poll.IsOfficial,
                Deadline = poll.Deadline,
                ClosedAt = poll.ClosedAt,
                Created = poll.Created,
                State = poll.State,
                WallId = poll.WallId,
                CreatedBy = await GetPersonAsync(poll.CreatedBy),
                Review = await GetReviewAsync(poll),
                VoterCount = await _participantDbSet.CountAsync(entity => entity.PollId == poll.Id),
                AudienceSize = await GetAudienceSizeAsync(userOrg.OrganizationId),
                VotedByMe = votedByMe,
                CanSeeResults = canSeeResults,
                QuestionCount = poll.Questions.Count
            };

            dto.Questions = await MapQuestionsAsync(poll, canSeeResults, userOrg.UserId);

            return dto;
        }

        public async Task<PollDto> CreateAsync(CreatePollDto dto, bool canManage)
        {
            ValidateContent(dto.Title, dto.Description, dto.Questions);

            var state = !canManage || dto.Suggest
                ? PollState.Pending
                : dto.Publish ? PollState.Published : PollState.Draft;

            if (state != PollState.Draft && dto.Deadline <= DateTime.UtcNow)
            {
                throw new ArgumentException("The deadline must be in the future.");
            }

            var wallId = await _wallService.CreateNewWallAsync(new CreateWallDto
            {
                Name = dto.Title,
                Description = dto.Description,
                Access = WallAccess.Private,
                Type = WallType.Polls,
                ModeratorsIds = new List<string> { dto.UserId },
                MembersIds = new List<string> { dto.UserId },
                UserId = dto.UserId,
                OrganizationId = dto.OrganizationId
            });

            var now = DateTime.UtcNow;
            var poll = new Poll
            {
                OrganizationId = dto.OrganizationId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                IsAnonymous = dto.IsAnonymous,
                IsOfficial = state != PollState.Pending && canManage && dto.IsOfficial,
                Deadline = dto.Deadline,
                State = state,
                WallId = wallId,
                Created = now,
                CreatedBy = dto.UserId,
                Modified = now,
                ModifiedBy = dto.UserId,
                Questions = BuildQuestions(dto.Questions, dto.UserId, now)
            };

            _pollDbSet.Add(poll);
            await _uow.SaveChangesAsync(false);

            return await GetPollAsync(poll.Id, dto, canManage);
        }

        public async Task UpdateAsync(UpdatePollDto dto, bool canManage)
        {
            ValidateContent(dto.Title, dto.Description, dto.Questions);

            var poll = await _pollDbSet
                .Include(entity => entity.Questions)
                .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(entity => entity.Id == dto.Id && entity.OrganizationId == dto.OrganizationId);

            if (poll == null)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            if (poll.State == PollState.Published && poll.ClosedAt == null &&
                dto.Deadline <= DateTime.UtcNow)
            {
                throw new ArgumentException("The deadline must be in the future. Close the poll instead.");
            }

            var isAuthor = poll.CreatedBy == dto.UserId;
            var isOwnSuggestion = isAuthor &&
                                  (poll.State == PollState.Pending || poll.State == PollState.Rejected);

            if (!canManage && !isOwnSuggestion)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            var hasVotes = await _participantDbSet.AnyAsync(entity => entity.PollId == poll.Id);
            if (hasVotes)
            {
                ValidateBallotUnchanged(poll, dto);
            }

            var now = DateTime.UtcNow;
            poll.Title = dto.Title.Trim();
            poll.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            var isSuggestion = poll.State == PollState.Pending || poll.State == PollState.Rejected;
            poll.IsOfficial = !isSuggestion && canManage && dto.IsOfficial;
            poll.Deadline = dto.Deadline;
            poll.Modified = now;
            poll.ModifiedBy = dto.UserId;

            if (isOwnSuggestion)
            {
                poll.State = PollState.Pending;
                poll.ReviewReason = null;
                poll.ReviewedById = null;
                poll.ReviewedAt = null;
            }

            // The wall carries the poll's name into comment notifications and emails, so keep it in step.
            var wall = await _wallDbSet.FirstOrDefaultAsync(entity => entity.Id == poll.WallId);
            if (wall != null)
            {
                wall.Name = poll.Title;
                wall.Description = poll.Description;
            }

            if (!hasVotes)
            {
                poll.IsAnonymous = dto.IsAnonymous;

                foreach (var question in poll.Questions.ToList())
                {
                    foreach (var option in question.Options.ToList())
                    {
                        _optionDbSet.Remove(option);
                    }

                    _questionDbSet.Remove(question);
                }

                foreach (var question in BuildQuestions(dto.Questions, dto.UserId, now))
                {
                    question.PollId = poll.Id;
                    _questionDbSet.Add(question);
                }
            }

            await _uow.SaveChangesAsync(false);
        }

        public async Task PublishAsync(PollReviewArgsDto args)
        {
            var poll = await GetForManageAsync(args.Id, args.OrganizationId);

            if (poll.State == PollState.Published)
            {
                throw new ArgumentException("This poll is already published.");
            }

            var reason = args.Reason?.Trim();
            var now = DateTime.UtcNow;

            if (poll.Deadline <= now)
            {
                throw new ArgumentException("The deadline has already passed. Give the poll a new deadline before publishing it.");
            }

            poll.State = PollState.Published;
            poll.ClosedAt = null;
            poll.ReviewReason = string.IsNullOrEmpty(reason) ? null : reason;
            poll.ReviewedById = string.IsNullOrEmpty(reason) ? null : args.UserId;
            poll.ReviewedAt = string.IsNullOrEmpty(reason) ? null : now;

            poll.Modified = now;
            poll.ModifiedBy = args.UserId;

            await _uow.SaveChangesAsync(false);
        }

        public async Task RejectAsync(PollReviewArgsDto args)
        {
            var reason = args.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentException("A reason is required.");
            }

            var poll = await GetForManageAsync(args.Id, args.OrganizationId);

            if (poll.State != PollState.Pending)
            {
                throw new ArgumentException("Only a suggestion awaiting review can be turned down.");
            }

            var now = DateTime.UtcNow;

            poll.State = PollState.Rejected;
            poll.ReviewReason = reason;
            poll.ReviewedById = args.UserId;
            poll.ReviewedAt = now;
            poll.Modified = now;
            poll.ModifiedBy = args.UserId;

            await _uow.SaveChangesAsync(false);
        }

        public async Task CloseAsync(int id, UserAndOrganizationDto userOrg)
        {
            var poll = await GetForManageAsync(id, userOrg.OrganizationId);

            if (poll.State != PollState.Published)
            {
                throw new ArgumentException("Only a published poll can be closed.");
            }

            var now = DateTime.UtcNow;
            poll.ClosedAt = now;
            poll.Modified = now;
            poll.ModifiedBy = userOrg.UserId;

            await _uow.SaveChangesAsync(false);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userOrg)
        {
            var poll = await GetForManageAsync(id, userOrg.OrganizationId);
            var wallId = poll.WallId;

            _pollDbSet.Remove(poll);
            await _uow.SaveChangesAsync(userOrg.UserId);

            await _wallService.DeleteWallAsync(wallId, userOrg, WallType.Polls);
        }

        public async Task VoteAsync(PollVoteDto dto)
        {
            var poll = await _pollDbSet
                .Include(entity => entity.Questions)
                .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(entity => entity.Id == dto.PollId && entity.OrganizationId == dto.OrganizationId);

            if (poll == null)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            if (poll.State != PollState.Published || IsClosed(poll))
            {
                throw new ArgumentException("This poll is not open for voting.");
            }

            if (await HasVotedAsync(poll.Id, dto.UserId))
            {
                throw new ArgumentException("You have already voted in this poll.");
            }

            var answers = dto.Answers ?? new List<PollQuestionAnswerDto>();
            var picks = new List<PollAnswer>();

            foreach (var question in poll.Questions)
            {
                var answer = answers.FirstOrDefault(entry => entry.QuestionId == question.Id);
                var optionIds = answer?.OptionIds?.Distinct().ToList() ?? new List<int>();

                if (optionIds.Count == 0)
                {
                    throw new ArgumentException("Every question must be answered.");
                }

                if (!question.AllowMultiple && optionIds.Count > 1)
                {
                    throw new ArgumentException("This question accepts a single answer.");
                }

                if (optionIds.Any(optionId => question.Options.All(option => option.Id != optionId)))
                {
                    throw new ArgumentException("An answer does not belong to its question.");
                }

                picks.AddRange(optionIds.Select(optionId => new PollAnswer
                {
                    Id = Guid.NewGuid(),
                    PollId = poll.Id,
                    PollQuestionId = question.Id,
                    PollOptionId = optionId,
                    ApplicationUserId = poll.IsAnonymous ? null : dto.UserId
                }));
            }

            _participantDbSet.Add(new PollParticipant
            {
                Id = Guid.NewGuid(),
                PollId = poll.Id,
                ApplicationUserId = dto.UserId
            });

            foreach (var pick in picks.OrderBy(_ => Guid.NewGuid()))
            {
                _answerDbSet.Add(pick);
            }

            if (!poll.IsAnonymous)
            {
                await _wallService.AddMemberToWallsAsync(dto.UserId, new List<int> { poll.WallId });
            }

            try
            {
                await _uow.SaveChangesAsync(dto.UserId);
            }
            catch (DbUpdateException e) when (IsUniqueViolation(e))
            {
                throw new ArgumentException("You have already voted in this poll.");
            }
        }

        private IQueryable<Poll> BaseQuery(UserAndOrganizationDto userOrg)
        {
            return _pollDbSet
                .AsNoTracking()
                .Where(poll => poll.OrganizationId == userOrg.OrganizationId)
                .OrderByDescending(poll => poll.Created);
        }

        private async Task<Poll> GetForManageAsync(int id, int organizationId)
        {
            var poll = await _pollDbSet
                .Include(entity => entity.Questions)
                .FirstOrDefaultAsync(entity => entity.Id == id && entity.OrganizationId == organizationId);

            if (poll == null)
            {
                throw new InvalidOperationException("Poll not found.");
            }

            return poll;
        }

        private async Task<IEnumerable<PollListItemDto>> MapListAsync(IList<Poll> polls, UserAndOrganizationDto userOrg)
        {
            var pollIds = polls.Select(poll => poll.Id).ToList();

            var voterCounts = await _participantDbSet
                .Where(entity => pollIds.Contains(entity.PollId))
                .GroupBy(entity => entity.PollId)
                .Select(group => new { PollId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(entry => entry.PollId, entry => entry.Count);

            var myVotes = await _participantDbSet
                .Where(entity => pollIds.Contains(entity.PollId) && entity.ApplicationUserId == userOrg.UserId)
                .Select(entity => entity.PollId)
                .ToListAsync();

            var questionCounts = await _questionDbSet
                .Where(entity => pollIds.Contains(entity.PollId))
                .GroupBy(entity => entity.PollId)
                .Select(group => new { PollId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(entry => entry.PollId, entry => entry.Count);

            var authorIds = polls.Select(poll => poll.CreatedBy)
                .Concat(polls.Where(poll => poll.ReviewedById != null).Select(poll => poll.ReviewedById))
                .Distinct()
                .ToList();

            var people = await GetPeopleAsync(authorIds);
            var audienceSize = await GetAudienceSizeAsync(userOrg.OrganizationId);

            return polls.Select(poll => new PollListItemDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                IsAnonymous = poll.IsAnonymous,
                IsOfficial = poll.IsOfficial,
                Deadline = poll.Deadline,
                ClosedAt = poll.ClosedAt,
                Created = poll.Created,
                State = poll.State,
                WallId = poll.WallId,
                CreatedBy = people.TryGetValue(poll.CreatedBy ?? string.Empty, out var author) ? author : null,
                Review = BuildReview(poll, people),
                QuestionCount = questionCounts.TryGetValue(poll.Id, out var questions) ? questions : 0,
                VoterCount = voterCounts.TryGetValue(poll.Id, out var voters) ? voters : 0,
                AudienceSize = audienceSize,
                VotedByMe = myVotes.Contains(poll.Id)
            }).ToList();
        }

        private async Task<IEnumerable<PollQuestionDto>> MapQuestionsAsync(Poll poll, bool canSeeResults, string userId)
        {
            var questions = poll.Questions.OrderBy(question => question.Order).ToList();

            if (!canSeeResults)
            {
                return questions.Select(question => new PollQuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    AllowMultiple = question.AllowMultiple,
                    Options = question.Options.OrderBy(option => option.Order).Select(option => new PollOptionDto
                    {
                        Id = option.Id,
                        Text = option.Text,
                        Voters = new List<PollPersonDto>()
                    }).ToList()
                }).ToList();
            }

            var answers = await _answerDbSet
                .AsNoTracking()
                .Where(answer => answer.PollId == poll.Id)
                .Select(answer => new { answer.PollQuestionId, answer.PollOptionId, answer.ApplicationUserId })
                .ToListAsync();

            var voterIds = answers
                .Where(answer => answer.ApplicationUserId != null)
                .Select(answer => answer.ApplicationUserId)
                .Distinct()
                .ToList();

            var people = await GetPeopleAsync(voterIds);
            var participantCount = await _participantDbSet.CountAsync(entry => entry.PollId == poll.Id);

            return questions.Select(question =>
            {
                var questionAnswers = answers.Where(answer => answer.PollQuestionId == question.Id).ToList();
                var respondents = poll.IsAnonymous
                    ? participantCount
                    : questionAnswers.Select(answer => answer.ApplicationUserId).Distinct().Count();

                return new PollQuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    AllowMultiple = question.AllowMultiple,
                    RespondentCount = respondents,
                    Options = question.Options.OrderBy(option => option.Order).Select(option =>
                    {
                        var optionAnswers = questionAnswers.Where(answer => answer.PollOptionId == option.Id).ToList();

                        return new PollOptionDto
                        {
                            Id = option.Id,
                            Text = option.Text,
                            VoteCount = optionAnswers.Count,
                            Picked = !poll.IsAnonymous && optionAnswers.Any(answer => answer.ApplicationUserId == userId),
                            Voters = poll.IsAnonymous
                                ? new List<PollPersonDto>()
                                : optionAnswers
                                    .Where(answer => answer.ApplicationUserId != null &&
                                                     people.ContainsKey(answer.ApplicationUserId))
                                    .Select(answer => people[answer.ApplicationUserId])
                                    .ToList()
                        };
                    }).ToList()
                };
            }).ToList();
        }

        private static ICollection<PollQuestion> BuildQuestions(IEnumerable<CreatePollQuestionDto> questions, string userId, DateTime now)
        {
            return questions.Select((question, questionIndex) => new PollQuestion
            {
                Text = question.Text.Trim(),
                AllowMultiple = question.AllowMultiple,
                Order = questionIndex,
                Created = now,
                CreatedBy = userId,
                Modified = now,
                ModifiedBy = userId,
                Options = question.Options
                    .Where(option => !string.IsNullOrWhiteSpace(option.Text))
                    .Select((option, optionIndex) => new PollOption
                    {
                        Text = option.Text.Trim(),
                        Order = optionIndex,
                        Created = now,
                        CreatedBy = userId,
                        Modified = now,
                        ModifiedBy = userId
                    }).ToList()
            }).ToList();
        }

        private static void ValidateBallotUnchanged(Poll poll, UpdatePollDto dto)
        {
            if (dto.IsAnonymous != poll.IsAnonymous)
            {
                throw new ArgumentException("Voting has started, so the anonymity setting can no longer be changed.");
            }

            var existing = poll.Questions.OrderBy(question => question.Order).ToList();

            if (existing.Count != dto.Questions.Count)
            {
                throw new ArgumentException(BallotLockedMessage);
            }

            for (var index = 0; index < existing.Count; index++)
            {
                var current = existing[index];
                var submitted = dto.Questions[index];

                if (current.Text != submitted.Text.Trim() || current.AllowMultiple != submitted.AllowMultiple)
                {
                    throw new ArgumentException(BallotLockedMessage);
                }

                var currentOptions = current.Options
                    .OrderBy(option => option.Order)
                    .Select(option => option.Text)
                    .ToList();

                var submittedOptions = submitted.Options
                    .Where(option => !string.IsNullOrWhiteSpace(option.Text))
                    .Select(option => option.Text.Trim())
                    .ToList();

                if (!currentOptions.SequenceEqual(submittedOptions, StringComparer.Ordinal))
                {
                    throw new ArgumentException(BallotLockedMessage);
                }
            }
        }

        private static void ValidateContent(string title, string description, IList<CreatePollQuestionDto> questions)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("A title is required.");
            }

            if (title.Trim().Length > Poll.MaxTitleLength)
            {
                throw new ArgumentException("The title is too long.");
            }

            if (description != null && description.Trim().Length > Poll.MaxDescriptionLength)
            {
                throw new ArgumentException("The description is too long.");
            }

            if (questions == null || questions.Count == 0)
            {
                throw new ArgumentException("At least one question is required.");
            }

            if (questions.Count > MaxQuestionsPerPoll)
            {
                throw new ArgumentException($"A poll can have at most {MaxQuestionsPerPoll} questions.");
            }

            foreach (var question in questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    throw new ArgumentException("Every question needs text.");
                }

                if (question.Text.Trim().Length > PollQuestion.MaxTextLength)
                {
                    throw new ArgumentException("A question is too long.");
                }

                var options = question.Options?
                    .Where(option => !string.IsNullOrWhiteSpace(option.Text))
                    .Select(option => option.Text.Trim())
                    .ToList() ?? new List<string>();

                if (options.Count < MinOptionsPerQuestion)
                {
                    throw new ArgumentException("Every question needs at least two answers.");
                }

                if (options.Count > MaxOptionsPerQuestion)
                {
                    throw new ArgumentException($"A question can have at most {MaxOptionsPerQuestion} answers.");
                }

                if (options.Any(option => option.Length > PollOption.MaxTextLength))
                {
                    throw new ArgumentException("An answer is too long.");
                }

                if (options.Select(option => option.ToLowerInvariant()).Distinct().Count() != options.Count)
                {
                    throw new ArgumentException("Answers must be different from one another.");
                }
            }
        }

        private static bool IsUniqueViolation(DbUpdateException e)
        {
            return e.InnerException is SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }

        private static bool IsClosed(Poll poll)
        {
            return poll.ClosedAt != null || poll.Deadline <= DateTime.UtcNow;
        }

        private async Task<bool> HasVotedAsync(int pollId, string userId)
        {
            return await _participantDbSet.AnyAsync(entity => entity.PollId == pollId && entity.ApplicationUserId == userId);
        }

        private async Task<int> GetAudienceSizeAsync(int organizationId)
        {
            return await _userDbSet.CountAsync(user => user.OrganizationId == organizationId && !user.IsDeleted);
        }

        private async Task<PollPersonDto> GetPersonAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var people = await GetPeopleAsync(new List<string> { userId });

            return people.TryGetValue(userId, out var person) ? person : null;
        }

        private async Task<PollReviewDto> GetReviewAsync(Poll poll)
        {
            if (poll.ReviewReason == null || poll.ReviewedAt == null)
            {
                return null;
            }

            return new PollReviewDto
            {
                Reason = poll.ReviewReason,
                At = poll.ReviewedAt.Value,
                By = await GetPersonAsync(poll.ReviewedById)
            };
        }

        private static PollReviewDto BuildReview(Poll poll, IDictionary<string, PollPersonDto> people)
        {
            if (poll.ReviewReason == null || poll.ReviewedAt == null)
            {
                return null;
            }

            return new PollReviewDto
            {
                Reason = poll.ReviewReason,
                At = poll.ReviewedAt.Value,
                By = poll.ReviewedById != null && people.TryGetValue(poll.ReviewedById, out var person) ? person : null
            };
        }

        private async Task<IDictionary<string, PollPersonDto>> GetPeopleAsync(IList<string> userIds)
        {
            var ids = userIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<string, PollPersonDto>();
            }

            return await _userDbSet
                .AsNoTracking()
                .Where(user => ids.Contains(user.Id))
                .Select(user => new PollPersonDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PictureId = user.PictureId
                })
                .ToDictionaryAsync(person => person.Id);
        }
    }
}
