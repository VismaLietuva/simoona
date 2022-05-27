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

        public async Task CheckIfFilterPresetExistsAsync(FilterPresetDto createDto)
        {
            var exists = await _filterPresetDbSet
                .AnyAsync(p => p.Name == createDto.Name && p.ForPage == createDto.Type);

            if (exists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
            }
        }

        public async Task CheckIfFilterItemsExistsAsync(FilterPresetDto createDto)
        {
            foreach (var item in createDto.Filters)
            {
                await CheckFilterPresetItemFilterTypesAsync(item);
            }
        }

        public void CheckIfFilterPresetItemsContainDuplicates(FilterPresetDto createDto)
        {
            foreach (var item in createDto.Filters)
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

        private async Task CheckFilterPresetItemFilterTypesAsync(FilterPresetItemDto presetItem)
        {
            switch (presetItem.ForType)
            {
                case FilterType.Kudos:
                    await CheckKudosFilterTypesAsync(presetItem);
                    break;
                case FilterType.Offices:
                    await CheckOfficeFilterTypesAsync(presetItem);
                    break;
                case FilterType.Events:
                    await CheckEventFilterTypesAsync(presetItem);
                    break;
            }
        }

        private async Task CheckOfficeFilterTypesAsync(FilterPresetItemDto presetItem)
        {
            var count = await _officeDbSet
                .Where(office => presetItem.Types.Contains(office.Id.ToString()))
                .CountAsync();

            if (count != presetItem.Types.Count())
            {
                throw new ValidationException(ErrorCodes.IncorrectFilterType, "Specified office filter type does not exists");
            }
        }

        private async Task CheckEventFilterTypesAsync(FilterPresetItemDto presetItem)
        {
            var count = await _eventTypeDbSet
                .Where(type => presetItem.Types.Contains(type.Id.ToString()))
                .CountAsync();

            if (count != presetItem.Types.Count())
            {
                throw new ValidationException(ErrorCodes.IncorrectFilterType, "Specified event filter type does not exists");
            }
        }

        private async Task CheckKudosFilterTypesAsync(FilterPresetItemDto presetItem)
        {
            var kudosTypeNames = await _kudosTypesDbSet
                .Select(type => type.Name)
                .ToListAsync();

            var containsCorrectTypes = presetItem.Types
                .All(typeName => kudosTypeNames.Contains(typeName));

            if (!containsCorrectTypes)
            {
                throw new ValidationException(ErrorCodes.IncorrectFilterType, "Specified Kudos filter does not exists");
            }
        }
    }
}
