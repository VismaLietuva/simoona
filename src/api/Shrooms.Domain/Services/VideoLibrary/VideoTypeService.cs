using Microsoft.Data.SqlClient;
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
    public class VideoTypeService : IVideoTypeService
    {
        private const string DuplicateTitleMessage = "Video type with that title already exists";

        private readonly DbSet<VideoType> _videoTypesDbSet;
        private readonly DbSet<VideoLibraryItem> _videosDbSet;
        private readonly IUnitOfWork2 _uow;

        public VideoTypeService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _videoTypesDbSet = uow.GetDbSet<VideoType>();
            _videosDbSet = uow.GetDbSet<VideoLibraryItem>();
        }

        public async Task<IEnumerable<VideoTypeDto>> GetVideoTypesAsync(UserAndOrganizationDto userOrg)
        {
            return await _videoTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId)
                .OrderBy(t => t.Title)
                .Select(MapVideoTypeToDto())
                .ToListAsync();
        }

        public async Task CreateVideoTypeAsync(VideoTypeDto videoType)
        {
            await ThrowIfTitleTakenAsync(videoType, null);

            _videoTypesDbSet.Add(new VideoType
            {
                Title = videoType.Title,
                CreatedBy = videoType.UserId,
                OrganizationId = videoType.OrganizationId
            });

            await SaveOrTranslateAsync(videoType.UserId);
        }

        public async Task UpdateVideoTypeAsync(VideoTypeDto videoType)
        {
            await ThrowIfTitleTakenAsync(videoType, videoType.Id);

            var type = await FindAsync(videoType.Id, videoType.OrganizationId);

            type.Title = videoType.Title;

            await SaveOrTranslateAsync(videoType.UserId);
        }

        public async Task RemoveVideoTypeAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await FindAsync(id, userOrg.OrganizationId);
            var videosUsingType = await _videosDbSet
                .CountAsync(v => v.OrganizationId == userOrg.OrganizationId && v.VideoTypeId == id && !v.IsDeleted);

            if (videosUsingType > 0)
            {
                throw new ValidationException(
                    ErrorCodes.DuplicatesIntolerable,
                    $"Video type is still used by {videosUsingType} video(s)");
            }

            var deletedVideosUsingType = await _videosDbSet
                .IgnoreQueryFilters()
                .Where(v => v.OrganizationId == userOrg.OrganizationId && v.VideoTypeId == id)
                .ToListAsync();

            foreach (var video in deletedVideosUsingType)
            {
                video.VideoTypeId = null;
            }

            type.IsDeleted = true;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task SaveOrTranslateAsync(string userId)
        {
            try
            {
                await _uow.SaveChangesAsync(userId);
            }
            catch (DbUpdateException e) when (IsUniqueViolation(e))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, DuplicateTitleMessage);
            }
        }

        private static bool IsUniqueViolation(DbUpdateException e)
        {
            return e.InnerException is SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }

        private async Task<VideoType> FindAsync(int id, int organizationId)
        {
            var type = await _videoTypesDbSet
                .FirstOrDefaultAsync(t => t.OrganizationId == organizationId && t.Id == id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Video type not found");
            }

            return type;
        }

        private async Task ThrowIfTitleTakenAsync(VideoTypeDto videoType, int? excludedId)
        {
            var alreadyExists = await _videoTypesDbSet
                .AnyAsync(t => t.OrganizationId == videoType.OrganizationId &&
                               t.Title == videoType.Title &&
                               (excludedId == null || t.Id != excludedId));

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, DuplicateTitleMessage);
            }
        }

        private static Expression<Func<VideoType, VideoTypeDto>> MapVideoTypeToDto()
        {
            return videoType => new VideoTypeDto
            {
                Id = videoType.Id,
                Title = videoType.Title,
                VideoCount = videoType.Videos.Count(v => !v.IsDeleted)
            };
        }
    }
}
