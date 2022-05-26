using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using System.Linq;
using System;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public class FilterPresetValidator : IFilterPresetValidator
    {
        private readonly IDbSet<FilterPreset> _filterPresetDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet; 

        public FilterPresetValidator(IUnitOfWork2 uow)
        {
            _filterPresetDbSet = uow.GetDbSet<FilterPreset>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
        }

        public async Task CheckIfFilterPresetExistsAsync(CreateFilterPresetDto createDto)
        {
            await CheckIfFilterPresetExistsAsync(createDto.Name, createDto.Type);
        }

        public async Task CheckIfFilterItemsExistsAsync(CreateFilterPresetDto createDto)
        {
            await CheckIfFilterItemsExistsAsync(createDto.Filters);
        }

        public void CheckIfFilterPresetItemsContainDuplicates(CreateFilterPresetDto createDto)
        {
            CheckIfFilterPresetItemsContainDuplicates(createDto.Filters);
        }

        private void CheckIfFilterPresetItemsContainDuplicates(IEnumerable<FilterPresetItemDto> presetItems)
        {
            foreach (var item in presetItems)
            {
                CheckFilterPresetItemsContainDuplicates(item);
            }
        }

        private async Task CheckIfFilterItemsExistsAsync(IEnumerable<FilterPresetItemDto> presetItems)
        {
            foreach (var item in presetItems)
            {
                await CheckFilterPresetItemFilterTypesAsync(item);
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
                    throw new NotImplementedException();
                    break;
                case FilterType.Events:
                    throw new NotImplementedException();
                    break;
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
                throw new ValidationException(ErrorCodes.IncorrectFilterType, "Specified filter does not exists");
            }
        }

        private async Task CheckIfFilterPresetExistsAsync(string name, PageType type)
        {
            var exists = await _filterPresetDbSet
                .AnyAsync(p => p.Name == name && p.ForPage == type);

            if (exists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
            }
        }
    }
}
