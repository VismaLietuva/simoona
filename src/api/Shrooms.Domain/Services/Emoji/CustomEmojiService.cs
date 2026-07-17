using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.ServiceValidators.Validators.Emoji;
using System;
using CustomEmojiEntity = Shrooms.DataLayer.EntityModels.Models.Emoji.CustomEmoji;

namespace Shrooms.Domain.Services.Emoji
{
    public class CustomEmojiService : ICustomEmojiService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IPictureService _pictureService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomEmojiValidator _validator;
        private readonly DbSet<CustomEmojiEntity> _customEmojisDbSet;
        private readonly ICustomCache<int, EmojiListCacheEntry> _emojiListCache;
        private readonly ICustomCache<int, long> _generationCache;

        public CustomEmojiService(
            IUnitOfWork2 uow,
            IPictureService pictureService,
            IPermissionService permissionService,
            ICustomEmojiValidator validator,
            ICustomCache<int, EmojiListCacheEntry> emojiListCache,
            ICustomCache<int, long> generationCache)
        {
            _uow = uow;
            _pictureService = pictureService;
            _permissionService = permissionService;
            _validator = validator;
            _emojiListCache = emojiListCache;
            _generationCache = generationCache;
            _customEmojisDbSet = uow.GetDbSet<CustomEmojiEntity>();
        }

        public async Task<CustomEmojiListDto> GetAllAsync(UserAndOrganizationDto userOrg, string tenantName)
        {
            var generation = GetGeneration(userOrg.OrganizationId);

            if (_emojiListCache.TryGetValue(userOrg.OrganizationId, out var entry) && entry.Generation == generation)
            {
                return entry.List;
            }

            _emojiListCache.TryRemoveEntry(userOrg.OrganizationId);

            var tenant = tenantName.ToLowerInvariant();

            var emojis = await _customEmojisDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var result = new CustomEmojiListDto
            {
                Emojis = emojis.Select(x => MapToDto(x, tenant)).ToList(),
                ETag = Guid.NewGuid().ToString("N")
            };

            if (GetGeneration(userOrg.OrganizationId) == generation)
            {
                _emojiListCache.TryAdd(userOrg.OrganizationId, new EmojiListCacheEntry { List = result, Generation = generation });
            }

            return result;
        }

        public async Task<CustomEmojiDto> CreateAsync(NewCustomEmojiDto emojiDto, UserAndOrganizationDto userOrg, string tenantName)
        {
            _validator.CheckNameFormat(emojiDto.Name);
            await _validator.CheckIfNameIsTakenAsync(emojiDto.Name, userOrg.OrganizationId);
            await _validator.CheckImageAsync(emojiDto.Content);

            var blobName = await _pictureService.UploadOriginalAsync(emojiDto.Content, emojiDto.MimeType, emojiDto.FileName, userOrg.OrganizationId);

            var emoji = new CustomEmojiEntity
            {
                Name = emojiDto.Name,
                BlobName = blobName,
                AuthorId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            _customEmojisDbSet.Add(emoji);
            await _uow.SaveChangesAsync(userOrg.UserId);
            BumpGeneration(userOrg.OrganizationId);
            _emojiListCache.TryRemoveEntry(userOrg.OrganizationId);

            return MapToDto(emoji, tenantName.ToLowerInvariant());
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userOrg)
        {
            var emoji = await _customEmojisDbSet
                .FirstOrDefaultAsync(x => x.Id == id && x.OrganizationId == userOrg.OrganizationId && !x.IsDeleted);

            if (emoji == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Emoji does not exist");
            }

            if (emoji.AuthorId != userOrg.UserId &&
                !await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.CustomEmoji))
            {
                throw new UnauthorizedException();
            }

            _customEmojisDbSet.Remove(emoji);
            await _uow.SaveChangesAsync(userOrg.UserId);
            BumpGeneration(userOrg.OrganizationId);
            _emojiListCache.TryRemoveEntry(userOrg.OrganizationId);
        }

        private long GetGeneration(int organizationId)
        {
            return _generationCache.TryGetValue(organizationId, out var generation) ? generation : 0;
        }

        private void BumpGeneration(int organizationId)
        {
            while (true)
            {
                if (_generationCache.TryGetValue(organizationId, out var current))
                {
                    _generationCache.TryRemoveEntry(organizationId);

                    if (_generationCache.TryAdd(organizationId, current + 1))
                    {
                        return;
                    }
                }
                else if (_generationCache.TryAdd(organizationId, 1))
                {
                    return;
                }
            }
        }

        private static CustomEmojiDto MapToDto(CustomEmojiEntity emoji, string tenant)
        {
            return new CustomEmojiDto
            {
                Id = emoji.Id,
                Name = emoji.Name,
                Url = $"/storage/{tenant}/{emoji.BlobName}",
                AuthorId = emoji.AuthorId
            };
        }
    }
}
