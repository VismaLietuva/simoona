using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public class FilterPresetValidator : IFilterPresetValidator
    {
        private readonly DbSet<FilterPreset> _filterPresetsDbSet;

        public FilterPresetValidator(IUnitOfWork2 uow)
        {
            _filterPresetsDbSet = uow.GetDbSet<FilterPreset>();
        }

        public void CheckIfPageTypeExists(PageType page)
        {
            if (!Enum.IsDefined(typeof(PageType), page))
            {
                throw new ValidationException(ErrorCodes.InvalidType, "Page does not exists");
            }
        }

        public void CheckIfFilterTypesContainsDuplicates(FilterType[] filterTypes)
        {
            var length = filterTypes.Length;

            if (filterTypes.Distinct().Count() != length)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
            }
        }

        public void CheckIfMoreThanOneDefaultPresetExists(AddEditDeleteFilterPresetDto updateDto)
        {
            var defaultPresetExistsInNewPresets = updateDto.PresetsToAdd.Any(preset => preset.IsDefault);
            var defaultPresetExistsInUpdatedPresets = updateDto.PresetsToUpdate.Any(preset => preset.IsDefault);

            if (defaultPresetExistsInNewPresets && defaultPresetExistsInUpdatedPresets)
            {
                throw new ValidationException(ErrorCodes.FilterPresetContainsMoreThanOneDefaultPreset,
                    "Cannot have more than one default preset");
            }
        }

        public void CheckIfCountsAreEqual<T1, T2>(T1 firstCollection, T2 secondCollection)
            where T1 : ICollection
            where T2 : ICollection
        {
            if (firstCollection.Count != secondCollection.Count)
            {
                throw new ValidationException(ErrorCodes.FilterNotFound, "Filter does not exists");
            }
        }

        public async void CheckIfUniqueNames(AddEditDeleteFilterPresetDto updateDto, IEnumerable<FilterPresetDto> removedPresets)
        {
            var names = updateDto
                .PresetsToAdd.Select(p => p.Name)
                .Union(updateDto.PresetsToUpdate.Select(p => p.Name))
                .ToList();

            if (names.Distinct().Count() != names.Count)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }

            var namesFromDb = await _filterPresetsDbSet
                .Where(preset => names.Contains(preset.Name))
                .Select(preset => preset.Name)
                .ToListAsync();

            if (namesFromDb.Count != 0 && removedPresets.Count() != namesFromDb.Count)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }
            
            if (!removedPresets.All(preset => namesFromDb.Contains(preset.Name)))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }
        }

        public void CheckIfFilterTypesAreValid(FilterType[] filterTypes)
        {
            foreach (var filterType in filterTypes)
            {
                CheckIfFilterTypeIsValid(filterType);
            }
        }

        private void CheckIfFilterTypeIsValid(FilterType filterType)
        {
            if (!Enum.IsDefined(typeof(FilterType), filterType))
            {
                throw new ValidationException(ErrorCodes.InvalidType, "Filter does not exists");
            }
        }
    }
}
