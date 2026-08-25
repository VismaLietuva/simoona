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

namespace Shrooms.Domain.Services.VideoLibrary
{
    public class VideoLibraryService : IVideoLibraryService
    {
        private readonly DbSet<VideoLibraryItem> _videosDbSet;
        private readonly DbSet<VideoType> _videoTypesDbSet;
        private readonly IUnitOfWork2 _uow;

        public VideoLibraryService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _videosDbSet = uow.GetDbSet<VideoLibraryItem>();
            _videoTypesDbSet = uow.GetDbSet<VideoType>();
        }

        public async Task<IEnumerable<VideoLibraryItemDto>> GetVideosAsync(UserAndOrganizationDto userOrg)
        {
            return await _videosDbSet
                .Where(v => v.OrganizationId == userOrg.OrganizationId)
                .OrderByDescending(v => v.Created)
                .Select(MapVideoToDto())
                .ToListAsync();
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

            entity.Title = video.Title;
            entity.Url = video.Url;
            entity.Description = video.Description;
            entity.PictureId = video.PictureId;
            entity.VideoTypeId = video.VideoTypeId;

            await _uow.SaveChangesAsync(video.UserId);
        }

        public async Task RemoveVideoAsync(int id, UserAndOrganizationDto userOrg)
        {
            var entity = await FindAsync(id, userOrg.OrganizationId);

            entity.IsDeleted = true;

            await _uow.SaveChangesAsync(userOrg.UserId);
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
