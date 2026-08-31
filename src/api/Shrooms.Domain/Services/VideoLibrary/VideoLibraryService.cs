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
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.Picture;
using X.PagedList;

namespace Shrooms.Domain.Services.VideoLibrary
{
    public class VideoLibraryService : IVideoLibraryService
    {
        private readonly DbSet<VideoLibraryItem> _videosDbSet;
        private readonly DbSet<VideoType> _videoTypesDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly IPictureService _pictureService;

        public VideoLibraryService(IUnitOfWork2 uow, IPictureService pictureService)
        {
            _uow = uow;
            _pictureService = pictureService;
            _videosDbSet = uow.GetDbSet<VideoLibraryItem>();
            _videoTypesDbSet = uow.GetDbSet<VideoType>();
        }

        public async Task<IPagedList<VideoLibraryItemDto>> GetVideosAsync(VideoLibraryListArgsDto args)
        {
            var query = _videosDbSet
                .Where(v => v.OrganizationId == args.OrganizationId)
                .Where(BuildTypeFilter(args))
                .Where(BuildSearchFilter(args))
                .OrderByDescending(v => v.Created)
                .Select(MapVideoToDto());

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((args.Page - 1) * args.PageSize)
                .Take(args.PageSize)
                .ToListAsync();

            return new StaticPagedList<VideoLibraryItemDto>(items, args.Page, args.PageSize, totalCount);
        }

        public async Task<VideoLibraryFiltersDto> GetFiltersAsync(UserAndOrganizationDto userOrg)
        {
            var videos = _videosDbSet.Where(v => v.OrganizationId == userOrg.OrganizationId);

            var types = await _videoTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId)
                .Select(t => new VideoTypeDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    VideoCount = t.Videos.Count(v => !v.IsDeleted)
                })
                .Where(t => t.VideoCount > 0)
                .OrderBy(t => t.Title)
                .ToListAsync();

            return new VideoLibraryFiltersDto
            {
                Types = types,
                UncategorisedCount = await videos.CountAsync(v => v.VideoTypeId == null),
                TotalCount = await videos.CountAsync()
            };
        }

        public async Task CreateVideoAsync(VideoLibraryItemDto video)
        {
            ValidateUrl(video.Url);
            await ValidateVideoTypeAsync(video.VideoTypeId, video.OrganizationId);

            _videosDbSet.Add(new VideoLibraryItem
            {
                Title = video.Title,
                Url = video.Url,
                Description = video.Description,
                PictureId = video.PictureId,
                VideoTypeId = video.VideoTypeId,
                CreatedBy = video.UserId,
                OrganizationId = video.OrganizationId
            });

            await _uow.SaveChangesAsync(video.UserId);
        }

        public async Task UpdateVideoAsync(VideoLibraryItemDto video)
        {
            ValidateUrl(video.Url);
            await ValidateVideoTypeAsync(video.VideoTypeId, video.OrganizationId);

            var entity = await FindAsync(video.Id, video.OrganizationId);
            var replacedPictureId = entity.PictureId;

            entity.Title = video.Title;
            entity.Url = video.Url;
            entity.Description = video.Description;
            entity.PictureId = video.PictureId;
            entity.VideoTypeId = video.VideoTypeId;

            await _uow.SaveChangesAsync(video.UserId);

            await RemoveReplacedPictureAsync(replacedPictureId, video.PictureId, video.OrganizationId);
        }

        public async Task RemoveVideoAsync(int id, UserAndOrganizationDto userOrg)
        {
            var entity = await FindAsync(id, userOrg.OrganizationId);

            entity.IsDeleted = true;

            await _uow.SaveChangesAsync(userOrg.UserId);
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

        private static Expression<Func<VideoLibraryItem, bool>> BuildSearchFilter(VideoLibraryListArgsDto args)
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

        private static void ValidateUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(ErrorCodes.InvalidType, "Video link must be an absolute http(s) URL");
            }
        }

        private static Expression<Func<VideoLibraryItem, VideoLibraryItemDto>> MapVideoToDto()
        {
            return video => new VideoLibraryItemDto
            {
                Id = video.Id,
                Title = video.Title,
                Url = video.Url,
                Description = video.Description,
                PictureId = video.PictureId,
                VideoTypeId = video.VideoTypeId,
                VideoTypeTitle = video.VideoType == null ? null : video.VideoType.Title,
                Created = video.Created
            };
        }
    }
}
