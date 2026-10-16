using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.Picture;
using X.PagedList;

namespace Shrooms.Domain.Services.VideoLibrary
{
    public class VideoLibraryService : IVideoLibraryService
    {
        public const int RecentVideoDays = 14;

        private readonly DbSet<VideoLibraryItem> _videosDbSet;
        private readonly DbSet<VideoType> _videoTypesDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly IPictureService _pictureService;
        private readonly ISystemClock _systemClock;

        public VideoLibraryService(IUnitOfWork2 uow, IPictureService pictureService, ISystemClock systemClock)
        {
            _uow = uow;
            _pictureService = pictureService;
            _systemClock = systemClock;
            _videosDbSet = uow.GetDbSet<VideoLibraryItem>();
            _videoTypesDbSet = uow.GetDbSet<VideoType>();
        }

        public async Task<IPagedList<VideoLibraryItemDto>> GetVideosAsync(VideoLibraryListArgsDto args)
        {
            // One cutoff for both the "recently added" filter and each video's IsNew.
            var since = RecentSince();

            var filtered = _videosDbSet
                .Where(v => v.OrganizationId == args.OrganizationId)
                .Where(BuildTypeFilter(args))
                .Where(BuildSearchFilter(args))
                .Where(BuildRecordedFromFilter(args))
                .Where(BuildRecordedToFilter(args))
                .Where(BuildRecentFilter(args, since));

            var ordered = args.Recent
                ? filtered
                    .OrderByDescending(v => v.Created)
                    .ThenByDescending(v => v.Id)
                : filtered
                    .OrderByDescending(v => v.RecordedOn)
                    .ThenByDescending(v => v.Created)
                    .ThenByDescending(v => v.Id);

            var query = ordered.Select(MapVideoToDto(since));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((args.Page - 1) * args.PageSize)
                .Take(args.PageSize)
                .ToListAsync();

            return new StaticPagedList<VideoLibraryItemDto>(items, args.Page, args.PageSize, totalCount);
        }

        public async Task<VideoLibraryFiltersDto> GetFiltersAsync(VideoLibraryFiltersArgsDto args)
        {
            var library = _videosDbSet.Where(v => v.OrganizationId == args.OrganizationId);
            var matching = library
                .Where(BuildSearchFilter(args))
                .Where(BuildRecordedFromFilter(args))
                .Where(BuildRecordedToFilter(args));
            var since = RecentSince();

            var countsByType = await matching
                .Where(v => v.VideoTypeId != null)
                .GroupBy(v => v.VideoTypeId.Value)
                .Select(g => new { TypeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TypeId, x => x.Count);

            var typesInUse = await _videoTypesDbSet
                .Where(t => t.OrganizationId == args.OrganizationId && t.Videos.Any(v => !v.IsDeleted))
                .OrderBy(t => t.Title)
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();

            // One aggregate each for the filtered counts and the whole-library flags,
            // rather than a round trip per number. No row comes back for an empty set.
            var matchingCounts = await matching
                .GroupBy(v => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Uncategorised = g.Count(v => v.VideoTypeId == null),
                    Recent = g.Count(v => v.Created >= since)
                })
                .FirstOrDefaultAsync();

            var libraryCounts = await library
                .GroupBy(v => 1)
                .Select(g => new
                {
                    Uncategorised = g.Count(v => v.VideoTypeId == null),
                    Recent = g.Count(v => v.Created >= since)
                })
                .FirstOrDefaultAsync();

            return new VideoLibraryFiltersDto
            {
                Types = typesInUse
                    .Select(t => new VideoTypeDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        VideoCount = countsByType.GetValueOrDefault(t.Id)
                    })
                    .ToList(),
                UncategorisedCount = matchingCounts?.Uncategorised ?? 0,
                HasUncategorised = libraryCounts?.Uncategorised > 0,
                RecentCount = matchingCounts?.Recent ?? 0,
                HasRecent = libraryCounts?.Recent > 0,
                TotalCount = matchingCounts?.Total ?? 0
            };
        }

        public async Task CreateVideoAsync(VideoLibraryItemDto video)
        {
            ValidateUrl(video.Url);
            ValidateRecordedOn(video.RecordedOn);
            await ValidateVideoTypeAsync(video.VideoTypeId, video.OrganizationId);

            _videosDbSet.Add(new VideoLibraryItem
            {
                Title = video.Title,
                Url = video.Url,
                Description = video.Description,
                PictureId = video.PictureId,
                RecordedOn = video.RecordedOn ?? Today(),
                VideoTypeId = video.VideoTypeId,
                CreatedBy = video.UserId,
                OrganizationId = video.OrganizationId
            });

            await _uow.SaveChangesAsync(video.UserId);
        }

        public async Task UpdateVideoAsync(VideoLibraryItemDto video)
        {
            ValidateUrl(video.Url);
            ValidateRecordedOn(video.RecordedOn);
            await ValidateVideoTypeAsync(video.VideoTypeId, video.OrganizationId);

            var entity = await FindAsync(video.Id, video.OrganizationId);
            var replacedPictureId = entity.PictureId;

            entity.Title = video.Title;
            entity.Url = video.Url;
            entity.Description = video.Description;
            entity.PictureId = video.PictureId;
            entity.VideoTypeId = video.VideoTypeId;

            if (video.RecordedOn != null)
            {
                entity.RecordedOn = video.RecordedOn.Value;
            }

            await _uow.SaveChangesAsync(video.UserId);

            await RemoveReplacedPictureAsync(replacedPictureId, video.PictureId, video.OrganizationId);
        }

        public async Task RemoveVideoAsync(int id, UserAndOrganizationDto userOrg)
        {
            var entity = await FindAsync(id, userOrg.OrganizationId);

            entity.IsDeleted = true;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private DateTime RecentSince()
        {
            return _systemClock.UtcNow.AddDays(-RecentVideoDays);
        }

        private static Expression<Func<VideoLibraryItem, bool>> BuildRecentFilter(VideoLibraryListArgsDto args, DateTime since)
        {
            if (!args.Recent)
            {
                return video => true;
            }

            return video => video.Created >= since;
        }

        private async Task RemoveReplacedPictureAsync(string previousPictureId, string currentPictureId, int organizationId)
        {
            if (string.IsNullOrEmpty(previousPictureId) || previousPictureId == currentPictureId)
            {
                return;
            }

            await _pictureService.RemoveImageAsync(previousPictureId, organizationId);
        }

        private static Expression<Func<VideoLibraryItem, bool>> BuildTypeFilter(VideoLibraryListArgsDto args)
        {
            if (args.Uncategorised)
            {
                return video => video.VideoTypeId == null;
            }

            if (args.VideoTypeId == null)
            {
                return video => true;
            }

            return video => video.VideoTypeId == args.VideoTypeId;
        }

        private static Expression<Func<VideoLibraryItem, bool>> BuildSearchFilter(VideoLibraryFiltersArgsDto args)
        {
            var search = args.Search?.Trim();

            if (string.IsNullOrEmpty(search))
            {
                return video => true;
            }

            return video => video.Title.Contains(search) ||
                            (video.Description != null && video.Description.Contains(search)) ||
                            (video.VideoType != null && video.VideoType.Title.Contains(search));
        }

        private static Expression<Func<VideoLibraryItem, bool>> BuildRecordedFromFilter(VideoLibraryFiltersArgsDto args)
        {
            if (args.From == null)
            {
                return video => true;
            }

            var from = args.From.Value;

            return video => video.RecordedOn >= from;
        }

        private static Expression<Func<VideoLibraryItem, bool>> BuildRecordedToFilter(VideoLibraryFiltersArgsDto args)
        {
            if (args.To == null)
            {
                return video => true;
            }

            var to = args.To.Value;

            return video => video.RecordedOn <= to;
        }

        private async Task<VideoLibraryItem> FindAsync(int id, int organizationId)
        {
            var entity = await _videosDbSet
                .FirstOrDefaultAsync(v => v.OrganizationId == organizationId && v.Id == id);

            if (entity == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Video not found");
            }

            return entity;
        }

        private async Task ValidateVideoTypeAsync(int? videoTypeId, int organizationId)
        {
            if (videoTypeId == null)
            {
                return;
            }

            var exists = await _videoTypesDbSet
                .AnyAsync(t => t.OrganizationId == organizationId && t.Id == videoTypeId);

            if (!exists)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Video type not found");
            }
        }

        private DateOnly Today()
        {
            return DateOnly.FromDateTime(_systemClock.UtcNow);
        }

        private void ValidateRecordedOn(DateOnly? recordedOn)
        {
            if (recordedOn > Today().AddDays(1))
            {
                throw new ValidationException(ErrorCodes.VideoRecordedInFuture, "Recording date cannot be in the future");
            }
        }

        private static void ValidateUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(ErrorCodes.VideoLinkInvalid, "Video link must be an absolute http(s) URL");
            }
        }

        private static Expression<Func<VideoLibraryItem, VideoLibraryItemDto>> MapVideoToDto(DateTime recentSince)
        {
            return video => new VideoLibraryItemDto
            {
                Id = video.Id,
                Title = video.Title,
                Url = video.Url,
                Description = video.Description,
                PictureId = video.PictureId,
                RecordedOn = video.RecordedOn,
                VideoTypeId = video.VideoTypeId,
                VideoTypeTitle = video.VideoType == null ? null : video.VideoType.Title,
                Created = video.Created,
                IsNew = video.Created >= recentSince
            };
        }
    }
}
