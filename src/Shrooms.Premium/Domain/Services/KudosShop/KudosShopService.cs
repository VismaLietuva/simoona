using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.KudosShop
{
    public class KudosShopService : IKudosShopService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<KudosShopItem> _kudosShopItemsDbSet;

        public KudosShopService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _kudosShopItemsDbSet = uow.GetDbSet<KudosShopItem>();
        }

        public async Task CreateItemAsync(KudosShopItemDto dto)
        {
            var alreadyExists = await _kudosShopItemsDbSet
                .AnyAsync(t => t.Name == dto.Name && t.OrganizationId == dto.OrganizationId);

            if (alreadyExists)
            {
                throw new ValidationException(PremiumErrorCodes.KudosShopItemAlreadyExist, "Kudos shop item already exists");
            }

            var modified = DateTime.UtcNow;

            var newItem = new KudosShopItem
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                CreatedBy = dto.UserId,
                Created = modified,
                Modified = modified,
                ModifiedBy = dto.UserId,
                OrganizationId = dto.OrganizationId,
                PictureId = dto.PictureId
            };

            _kudosShopItemsDbSet.Add(newItem);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task<KudosShopItemDto> GetItemAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await _kudosShopItemsDbSet
                .Where(t => t.Id == id && t.OrganizationId == userOrg.OrganizationId)
                .Select(t => new KudosShopItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Price = t.Price,
                    Description = t.Description,
                    PictureId = t.PictureId
                })
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            return type;
        }

        public async Task<bool> ItemsExistAsync(UserAndOrganizationDto userOrg)
        {
            return await _kudosShopItemsDbSet.AnyAsync(t => t.OrganizationId == userOrg.OrganizationId);
        }

        public async Task<IEnumerable<KudosShopItemDto>> GetAllItemsAsync(UserAndOrganizationDto userOrg)
        {
            var kudosTypes = await _kudosShopItemsDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId)
                .Select(MapKudosShopItemToKudosShopItemDto())
                .ToListAsync();

            return kudosTypes;
        }

        public async Task UpdateItemAsync(KudosShopItemDto dto)
        {
            var alreadyExists = await _kudosShopItemsDbSet
                .AnyAsync(t => t.Name == dto.Name && t.OrganizationId == dto.OrganizationId && t.Id != dto.Id);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Kudos shop item with that name already exists");
            }

            var itemToUpdate = await _kudosShopItemsDbSet
                 .FirstOrDefaultAsync(e => e.Id == dto.Id && e.OrganizationId == dto.OrganizationId);

            if (itemToUpdate == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            itemToUpdate.Name = dto.Name;
            itemToUpdate.Price = dto.Price;
            itemToUpdate.Description = dto.Description;
            itemToUpdate.PictureId = dto.PictureId;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task DeleteItemAsync(int id, UserAndOrganizationDto userOrg)
        {
            var item = await _kudosShopItemsDbSet
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId);

            if (item == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            _kudosShopItemsDbSet.Remove(item);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private static Expression<Func<KudosShopItem, KudosShopItemDto>> MapKudosShopItemToKudosShopItemDto()
        {
            return kudosShopItem => new KudosShopItemDto
            {
                Id = kudosShopItem.Id,
                Name = kudosShopItem.Name,
                Price = kudosShopItem.Price,
                Description = kudosShopItem.Description,
                OrganizationId = kudosShopItem.OrganizationId,
                UserId = kudosShopItem.CreatedBy,
                PictureId = kudosShopItem.PictureId
            };
        }
    }
}
