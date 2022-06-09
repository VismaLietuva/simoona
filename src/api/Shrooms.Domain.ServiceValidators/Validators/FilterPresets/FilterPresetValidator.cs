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
using System.Collections;
using System.Collections.Generic;

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

        public void CheckIfFilterTypeIsValid(FilterType filterType)
        {
            if (!Enum.IsDefined(typeof(FilterType), filterType))
            {
                throw new ValidationException(ErrorCodes.InvalidType, "Filter does not exists");
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

        //public async Task CheckIfFilterPresetExistsAsync(int id, UserAndOrganizationDto userOrg)
        //{
        //    var exists = await _filterPresetDbSet
        //        .AnyAsync(p => p.Id == id && p.OrganizationId == userOrg.OrganizationId);

        //    if (!exists)
        //    {
        //        throw new ValidationException(ErrorCodes.FilterNotFound, "Filter preset does not exists");
        //    }
        //}

        //public async Task CheckIfFilterPresetExistsAsync(FilterPresetDto presetDto)
        //{
        //    var exists = await _filterPresetDbSet
        //        .AnyAsync(p => p.Name == presetDto.Name &&
        //                  p.ForPage == presetDto.PageType &&
        //                  p.OrganizationId == presetDto.OrganizationId);

        //    if (exists)
        //    {
        //        throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Duplicates are not allowed");
        //    }
        //}

        //public async Task CheckIfFilterItemsExistsAsync(FilterPresetDto presetDto)
        //{
        //    foreach (var item in presetDto.Filters)
        //    {
        //        await CheckFilterPresetItemFilterTypesAsync(item, presetDto.OrganizationId);
        //    }
        //}

        //public void CheckIfFilterPresetItemsContainDuplicates(FilterPresetDto presetDto)
        //{
        //    foreach (var item in presetDto.Filters)
        //    {
        //        CheckFilterPresetItemsContainDuplicates(item);
        //    }
        //}

        //private void CheckFilterPresetItemsContainDuplicates(FilterPresetItemDto presetItem)
        //{
        //    var currentCount = presetItem.Types.Count();
        //    var distinctCount = presetItem.Types.Distinct().Count();

        //    if (currentCount != distinctCount)
        //    {
        //        throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Filter sequence cannot contain duplicates");
        //    }
        //}

        //private async Task CheckFilterPresetItemFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        //{
        //    switch (presetItem.FilterType)
        //    {
        //        case FilterType.Kudos:
        //            await CheckKudosFilterTypesAsync(presetItem);
        //            break;
        //        case FilterType.Offices:
        //            await CheckOfficeFilterTypesAsync(presetItem, organizationId);
        //            break;
        //        case FilterType.Events:
        //            await CheckEventFilterTypesAsync(presetItem, organizationId);
        //            break;
        //    }
        //}

        //private async Task CheckOfficeFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        //{
        //    var count = await _officeDbSet
        //        .Where(office => presetItem.Types.Contains(office.Id.ToString()) &&
        //                         office.OrganizationId == organizationId)
        //        .CountAsync();

        //    if (count != presetItem.Types.Count())
        //    {
        //        throw new ValidationException(ErrorCodes.InvalidType, "Specified office filter type does not exists");
        //    }
        //}

        //private async Task CheckEventFilterTypesAsync(FilterPresetItemDto presetItem, int organizationId)
        //{
        //    var count = await _eventTypeDbSet
        //        .Where(type => presetItem.Types.Contains(type.Id.ToString()) &&
        //                       type.OrganizationId == organizationId)
        //        .CountAsync();

        //    if (count != presetItem.Types.Count())
        //    {
        //        throw new ValidationException(ErrorCodes.InvalidType, "Specified event filter type does not exists");
        //    }
        //}

        //// Kudos types does not have an OrganizationId?
        //private async Task CheckKudosFilterTypesAsync(FilterPresetItemDto presetItem)
        //{
        //    var kudosTypeNames = await _kudosTypesDbSet
        //        .Select(type => type.Name)
        //        .ToListAsync();

        //    var containsCorrectTypes = presetItem.Types
        //        .All(typeName => kudosTypeNames.Contains(typeName));

        //    if (!containsCorrectTypes)
        //    {
        //        throw new ValidationException(ErrorCodes.InvalidType, "Specified Kudos filter does not exists");
        //    }
        //}

        public void CheckIfUniqueNames(AddEditDeleteFilterPresetDto updateDto)
        {
            var names = updateDto
                .PresetsToAdd.Select(p => p.Name)
                .Union(updateDto.PresetsToUpdate.Select(p => p.Name))
                .ToList();

            if (names.Distinct().Count() != names.Count)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Preset names cannot contain duplicates");
            }
        }
    }
}
