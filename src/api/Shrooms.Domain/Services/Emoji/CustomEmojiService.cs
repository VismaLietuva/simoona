using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.ServiceValidators.Validators.Emoji;
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
        private readonly DbSet<Organization> _organizationsDbSet;

        public CustomEmojiService(
            IUnitOfWork2 uow,
            IPictureService pictureService,
            IPermissionService permissionService,
            ICustomEmojiValidator validator)
        {
            _uow = uow;
            _pictureService = pictureService;
            _permissionService = permissionService;
            _validator = validator;
            _customEmojisDbSet = uow.GetDbSet<CustomEmojiEntity>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<IEnumerable<CustomEmojiDto>> GetAllAsync(UserAndOrganizationDto userOrg)
        {
            var tenant = await GetTenantContainerAsync(userOrg.OrganizationId);

            var emojis = await _customEmojisDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return emojis.Select(x => MapToDto(x, tenant)).ToList();
        }

        public async Task<CustomEmojiDto> CreateAsync(string name, Stream stream, string mimeType, string fileName, UserAndOrganizationDto userOrg)
        {
            _validator.CheckNameFormat(name);
            await _validator.CheckIfNameIsTakenAsync(name, userOrg.OrganizationId);

            var blobName = await _pictureService.UploadFromStreamAsync(stream, mimeType, fileName, userOrg.OrganizationId);

            var emoji = new CustomEmojiEntity
            {
                Name = name,
                BlobName = blobName,
                AuthorId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            _customEmojisDbSet.Add(emoji);
            await _uow.SaveChangesAsync(userOrg.UserId);

            var tenant = await GetTenantContainerAsync(userOrg.OrganizationId);

            return MapToDto(emoji, tenant);
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

        private async Task<string> GetTenantContainerAsync(int organizationId)
        {
            var organization = await _organizationsDbSet.FirstAsync(x => x.Id == organizationId);

            return organization.ShortName.ToLowerInvariant();
        }
    }
}
