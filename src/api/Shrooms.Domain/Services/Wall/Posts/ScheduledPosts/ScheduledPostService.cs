using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using WallEntity = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Wall.Posts.ScheduledPosts
{
    public class ScheduledPostService : IScheduledPostService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IPostService _postService;
        private readonly IJobScheduler _jobScheduler;
        private readonly ISystemClock _systemClock;
        private readonly DbSet<ScheduledPost> _scheduledPostsDbSet;
        private readonly DbSet<WallEntity> _wallsDbSet;

        public ScheduledPostService(IUnitOfWork2 uow, IPostService postService, IJobScheduler jobScheduler, ISystemClock systemClock)
        {
            _uow = uow;
            _postService = postService;
            _jobScheduler = jobScheduler;
            _systemClock = systemClock;
            _scheduledPostsDbSet = uow.GetDbSet<ScheduledPost>();
            _wallsDbSet = uow.GetDbSet<WallEntity>();
        }

        public async Task<IEnumerable<ScheduledPostDto>> GetScheduledPostsAsync(int? wallId, UserAndOrganizationDto userOrg)
        {
            var scheduledPosts = await _scheduledPostsDbSet
                .AsNoTracking()
                .Include(x => x.Wall)
                .Where(x => x.OrganizationId == userOrg.OrganizationId && x.AuthorId == userOrg.UserId)
                .Where(x => wallId == null || x.WallId == wallId)
                .OrderBy(x => x.ScheduledFor)
                .ToListAsync();

            return scheduledPosts.Select(x => MapToDto(x, x.Wall.Name)).ToList();
        }

        public async Task<ScheduledPostDto> CreateAsync(NewScheduledPostDto dto)
        {
            var wall = await _wallsDbSet.FirstOrDefaultAsync(x => x.Id == dto.WallId && x.OrganizationId == dto.OrganizationId)
                ?? throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");

            var scheduledFor = ToUtc(dto.ScheduledFor);
            EnsureInFuture(scheduledFor);
            ValidateContent(dto.MessageBody, dto.Images);

            var scheduledPost = new ScheduledPost
            {
                OrganizationId = dto.OrganizationId,
                AuthorId = dto.UserId,
                WallId = wall.Id,
                MessageBody = dto.MessageBody,
                Images = new ImageCollection(dto.Images),
                ScheduledFor = scheduledFor,
                Version = Guid.NewGuid()
            };

            _scheduledPostsDbSet.Add(scheduledPost);
            await _uow.SaveChangesAsync(dto.UserId);

            try
            {
                scheduledPost.JobId = ScheduleJob(scheduledPost);
            }
            catch
            {
                _scheduledPostsDbSet.Remove(scheduledPost);
                await _uow.SaveChangesAsync(dto.UserId);
                throw;
            }

            await _uow.SaveChangesAsync(dto.UserId);

            return MapToDto(scheduledPost, wall.Name);
        }

        public async Task<ScheduledPostDto> EditAsync(EditScheduledPostDto dto)
        {
            var scheduledPost = await GetOwnScheduledPostAsync(dto.Id, dto);
            var scheduledFor = ToUtc(dto.ScheduledFor);

            ValidateContent(dto.MessageBody, dto.Images);

            scheduledPost.MessageBody = dto.MessageBody;
            scheduledPost.Images = new ImageCollection(dto.Images);
            scheduledPost.Version = Guid.NewGuid();

            var previousJobId = scheduledPost.JobId;
            var isRescheduled = scheduledPost.ScheduledFor != scheduledFor;

            if (isRescheduled)
            {
                EnsureInFuture(scheduledFor);

                scheduledPost.ScheduledFor = scheduledFor;
                scheduledPost.JobId = ScheduleJob(scheduledPost);
            }

            await _uow.SaveChangesAsync(dto.UserId);

            if (isRescheduled)
            {
                _jobScheduler.Delete(previousJobId);
            }

            return MapToDto(scheduledPost, scheduledPost.Wall.Name);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userOrg)
        {
            var scheduledPost = await GetOwnScheduledPostAsync(id, userOrg);

            _scheduledPostsDbSet.Remove(scheduledPost);
            await _uow.SaveChangesAsync(userOrg.UserId);

            _jobScheduler.Delete(scheduledPost.JobId);
        }

        public async Task<NewlyCreatedPostDto> PublishNowAsync(int id, UserAndOrganizationDto userOrg)
        {
            var scheduledPost = await GetOwnScheduledPostAsync(id, userOrg);

            return await PublishAsync(scheduledPost);
        }

        public async Task<NewlyCreatedPostDto> PublishDueAsync(int id)
        {
            var scheduledPost = await _scheduledPostsDbSet.FirstOrDefaultAsync(x => x.Id == id);

            if (scheduledPost == null)
            {
                return null;
            }

            if (scheduledPost.ScheduledFor > _systemClock.UtcNow)
            {
                ScheduleJob(scheduledPost);
                return null;
            }

            return await PublishAsync(scheduledPost);
        }

        private async Task<NewlyCreatedPostDto> PublishAsync(ScheduledPost scheduledPost)
        {
            _scheduledPostsDbSet.Remove(scheduledPost);

            try
            {
                var createdPost = await _postService.CreateNewPostAsync(new NewPostDto
                {
                    UserId = scheduledPost.AuthorId,
                    OrganizationId = scheduledPost.OrganizationId,
                    WallId = scheduledPost.WallId,
                    MessageBody = scheduledPost.MessageBody,
                    Images = scheduledPost.Images
                });

                await _uow.SaveChangesAsync(scheduledPost.AuthorId);

                return createdPost;
            }
            catch (ValidationException)
            {
                await _uow.SaveChangesAsync(scheduledPost.AuthorId);

                return null;
            }
        }

        private async Task<ScheduledPost> GetOwnScheduledPostAsync(int id, UserAndOrganizationDto userOrg)
        {
            return await _scheduledPostsDbSet
                .Include(x => x.Wall)
                .FirstOrDefaultAsync(x => x.Id == id && x.OrganizationId == userOrg.OrganizationId && x.AuthorId == userOrg.UserId)
                ?? throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Scheduled post not found");
        }

        private string ScheduleJob(ScheduledPost scheduledPost)
        {
            var id = scheduledPost.Id;
            var organizationId = scheduledPost.OrganizationId;
            var tenantName = _uow.ConnectionName;

            return _jobScheduler.Schedule<IScheduledPostPublisher>(
                publisher => publisher.PublishAsync(id, organizationId, tenantName),
                new DateTimeOffset(scheduledPost.ScheduledFor, TimeSpan.Zero));
        }

        private void EnsureInFuture(DateTime scheduledFor)
        {
            if (scheduledFor <= _systemClock.UtcNow)
            {
                throw new ValidationException(ErrorCodes.ScheduledPostTimeInPast, "Scheduled time must be in the future");
            }
        }

        private static void ValidateContent(string messageBody, IEnumerable<string> images)
        {
            var isEmpty = string.IsNullOrWhiteSpace(messageBody) && !(images?.Any(image => !string.IsNullOrEmpty(image)) ?? false);

            if (isEmpty || messageBody?.Length > ValidationConstants.MaxPostMessageBodyLength)
            {
                throw new ValidationException(ErrorCodes.ScheduledPostContentInvalid, "Post must have a message or an image");
            }
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        }

        private static ScheduledPostDto MapToDto(ScheduledPost scheduledPost, string wallName)
        {
            return new ScheduledPostDto
            {
                Id = scheduledPost.Id,
                WallId = scheduledPost.WallId,
                WallName = wallName,
                MessageBody = scheduledPost.MessageBody,
                Images = scheduledPost.Images,
                ScheduledFor = DateTime.SpecifyKind(scheduledPost.ScheduledFor, DateTimeKind.Utc)
            };
        }
    }
}
