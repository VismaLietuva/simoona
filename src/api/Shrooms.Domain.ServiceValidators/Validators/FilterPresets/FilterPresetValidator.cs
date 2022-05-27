using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using System.Linq;
using System;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public class FilterPresetValidator : IFilterPresetValidator
    {
        private readonly IDbSet<FilterPreset> _filterPresetDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet;
        private readonly IDbSet<Office> _officeDbSet;
        private readonly IDbSet<EventType> _eventTypeDbSet;

        public FilterPresetValidator(IUnitOfWork2 uow)
        {
            _filterPresetDbSet = uow.GetDbSet<FilterPreset>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _officeDbSet = uow.GetDbSet<Office>();
            _eventTypeDbSet = uow.GetDbSet<EventType>();
        }

        public void CheckIfPageTypeExists(PageType page)
        {
            if (!Enum.IsDefined(typeof(PageType), page))
            {
                throw new ValidationException(ErrorCodes.IncorrectType, "Page does not exists");
            }
        }


        public async Task CheckIfFilterPresetExistsAsync(int id, UserAndOrganizationDto userOrg)
        {
            var exists = await _filterPresetDbSet
                .AnyAsync(p => p.Id == id && p.OrganizationId == userOrg.OrganizationId);

            if (!exists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Filter preset does not exists");
            }
        }

        public async Task CheckIfFilterPresetExistsAsync(FilterPresetDto presetDto)
        {
            var exists = await _filterPresetDbSet
                .AnyAsync(p => p.Name == presetDto.Name &&
                          p.ForPage == presetDto.ForPage &&
                          p.OrganizationId == presetDto.OrganizationId);

            if (exists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
            }
        }

        public async Task CheckIfFilterItemsExistsAsync(FilterPresetDto presetDto)
        {
            foreach (var item in presetDto.Filters)
            {
                await CheckFilterPresetItemFilterTypesAsync(item, presetDto.OrganizationId);
            }
        }

        public void CheckIfFilterPresetItemsContainDuplicates(FilterPresetDto presetDto)
        {
            foreach (var item in presetDto.Filters)
            {
                CheckFilterPresetItemsContainDuplicates(item);
            }
        }

        private void CheckFilterPresetItemsContainDuplicates(FilterPresetItemDto presetItem)
        {
            var currentCount = presetItem.Types.Count();
            var distinctCount = presetItem.Types.Distinct().Count();

            if (currentCount != distinctCount)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Filter sequence cannot contain duplicates");
            }
        }

        private async Task CheckFilterPresetItemFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        {
            switch (presetItem.ForType)
            {
                case FilterType.Kudos:
                    await CheckKudosFilterTypesAsync(presetItem);
                    break;
                case FilterType.Offices:
                    await CheckOfficeFilterTypesAsync(presetItem, organizationId);
                    break;
                case FilterType.Events:
                    await CheckEventFilterTypesAsync(presetItem, organizationId);
                    break;
            }
        }

        private async Task CheckOfficeFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        {
            var count = await _officeDbSet
                .Where(office => presetItem.Types.Contains(office.Id.ToString()) &&
                                 office.OrganizationId == organizationId)
                .CountAsync();

            if (count != presetItem.Types.Count())
            {
                throw new ValidationException(ErrorCodes.IncorrectType, "Specified office filter type does not exists");
            }
        }

        private async Task CheckEventFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        {
            var count = await _eventTypeDbSet
                .Where(type => presetItem.Types.Contains(type.Id.ToString()) &&
                               type.OrganizationId == organizationId)
                .CountAsync();

            if (count != presetItem.Types.Count())
            {
                throw new ValidationException(ErrorCodes.IncorrectType, "Specified event filter type does not exists");
            }
        }

        // Kudos types does not have an OrganizationId?
        private async Task CheckKudosFilterTypesAsync(FilterPresetItemDto presetItem)
        {
            var kudosTypeNames = await _kudosTypesDbSet
                .Select(type => type.Name)
                .ToListAsync();

            var containsCorrectTypes = presetItem.Types
                .All(typeName => kudosTypeNames.Contains(typeName));

            if (!containsCorrectTypes)
            {
                throw new ValidationException(ErrorCodes.IncorrectType, "Specified Kudos filter does not exists");
            }
        }
    }
}
