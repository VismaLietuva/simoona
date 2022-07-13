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
using System.Threading.Tasks;

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

        public void CheckIfFilterTypesContainDuplicates(FilterType[] filterTypes)
        {
            var length = filterTypes.Length;

            if (filterTypes.Distinct().Count() != length)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
            }
        }

        public void CheckIfMoreThanOneDefaultPresetExists(ManageFilterPresetDto manageFilterPresetDto)
        {
            var defaultPresetExistsInNewPresets = manageFilterPresetDto.PresetsToCreate
                .Where(preset => preset.IsDefault)
                .ToList();

            var defaultPresetExistsInUpdatedPresets = manageFilterPresetDto.PresetsToUpdate
                .Where(preset => preset.IsDefault)
                .ToList();

            if (defaultPresetExistsInNewPresets.Count > 1 ||
                defaultPresetExistsInUpdatedPresets.Count > 1 ||
                (defaultPresetExistsInNewPresets.Count > 0 && defaultPresetExistsInUpdatedPresets.Count > 0))
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

        public void CheckIfFilterPresetsContainUniqueNames(IEnumerable<FilterPresetDto> filterPresetDtos)
        {
            var presetDtos = filterPresetDtos.ToList();

            if (presetDtos.GroupBy(preset => preset.Name).Count() != presetDtos.Count())
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }
        }

        public async Task CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(ManageFilterPresetDto manageFilterPresetDto, IEnumerable<FilterPresetDto> removedPresets)
        {
            var updateDtoContainsNameDuplicates = manageFilterPresetDto.PresetsToUpdate
                .Any(updatePreset => manageFilterPresetDto.PresetsToCreate.Any(addPreset => addPreset.Name == updatePreset.Name));

            if (updateDtoContainsNameDuplicates)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }

            var presets = await _filterPresetsDbSet
                .Where(preset => preset.ForPage == manageFilterPresetDto.PageType)
                .ToListAsync();

            var removedPresetNames = removedPresets
                .Select(preset => preset.Name)
                .ToList();

            if (manageFilterPresetDto.PresetsToCreate.Any(preset => presets.Any(dbPreset => dbPreset.Name == preset.Name) && !removedPresetNames.Contains(preset.Name)) ||
                manageFilterPresetDto.PresetsToUpdate.Any(preset => presets.Any(dbPreset => dbPreset.Name == preset.Name && dbPreset.Id != preset.Id) && !removedPresetNames.Contains(preset.Name)))
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
