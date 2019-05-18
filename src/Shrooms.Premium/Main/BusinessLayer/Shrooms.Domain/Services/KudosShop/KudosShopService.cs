using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using ErrorCodes = Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes.ErrorCodes;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.KudosShop
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

        public async Task CreateItem(KudosShopItemDTO dto)
        {
            var alreadyExists = await _kudosShopItemsDbSet
                .AnyAsync(t => t.Name == dto.Name && t.OrganizationId == dto.OrganizationId);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.KudosShopItemAlreadyExist, "Kudos shop item already exists");
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

        public async Task<KudosShopItemDTO> GetItem(int id, UserAndOrganizationDTO userOrg)
        {
            var type = await _kudosShopItemsDbSet
                .Where(t => t.Id == id && t.OrganizationId == userOrg.OrganizationId)
                .Select(t => new KudosShopItemDTO
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
                throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            return type;
        }

        public bool ItemsExist(UserAndOrganizationDTO userOrg)
        {
            return _kudosShopItemsDbSet.Any(t => t.OrganizationId == userOrg.OrganizationId);
        }

        public async Task<IEnumerable<KudosShopItemDTO>> GetAllItems(UserAndOrganizationDTO userOrg)
        {
            var kudosTypesDTO = await _kudosShopItemsDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId)
                .Select(MapKudosShopItemToKudosShopItemDTO())
                .ToListAsync();

            return kudosTypesDTO;
        }

        public async Task UpdateItem(KudosShopItemDTO dto)
        {
            var alreadyExists = await _kudosShopItemsDbSet
                .AnyAsync(t => t.Name == dto.Name && t.OrganizationId == dto.OrganizationId && t.Id != dto.Id);

            if (alreadyExists)
            {
                throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.DuplicatesIntolerable, "Kudos shop item with that name already exists");
            }

            var itemToUpdate = await _kudosShopItemsDbSet
                 .FirstOrDefaultAsync(e => e.Id == dto.Id && e.OrganizationId == dto.OrganizationId);

            if (itemToUpdate == null)
            {
                throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            itemToUpdate.Name = dto.Name;
            itemToUpdate.Price = dto.Price;
            itemToUpdate.Description = dto.Description;
            itemToUpdate.PictureId = dto.PictureId;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task DeleteItem(int id, UserAndOrganizationDTO userOrg)
        {
            var item = await _kudosShopItemsDbSet
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId);

            if (item == null)
            {
                throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.ContentDoesNotExist, "Item not found");
            }

            _kudosShopItemsDbSet.Remove(item);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private Expression<Func<KudosShopItem, KudosShopItemDTO>> MapKudosShopItemToKudosShopItemDTO()
        {
            return kudosShopItem => new KudosShopItemDTO()
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
