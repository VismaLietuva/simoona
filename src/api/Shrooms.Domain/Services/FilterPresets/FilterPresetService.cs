using Newtonsoft.Json;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using System;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.FilterPresets
{
    public class FilterPresetService : IFilterPresetService
    {
        private static readonly SemaphoreSlim _filterPresetUpdateCreateLock = new SemaphoreSlim(1, 1);

        private readonly IUnitOfWork2 _uow;

        private readonly DbSet<FilterPreset> _filterPresetDbSet;

        private readonly IDbSet<KudosType> _kudosTypeDbSet;
        private readonly IDbSet<Office> _officeDbSet;
        private readonly IDbSet<EventType> _eventTypeDbSet;

        private readonly IFilterPresetValidator _validator;

        public FilterPresetService(IUnitOfWork2 uow, IFilterPresetValidator validator)
        {
            _uow = uow;
            _validator = validator;

            _filterPresetDbSet = uow.GetDbSet<FilterPreset>();
            _kudosTypeDbSet = uow.GetDbSet<KudosType>();
            _officeDbSet = uow.GetDbSet<Office>();
            _eventTypeDbSet = uow.GetDbSet<EventType>();
        }

        public bool UpdatingFilterPresets 
        {
            get => _filterPresetUpdateCreateLock.CurrentCount == 0;
        }

        public async Task<IEnumerable<FilterPresetDto>> GetPresetsForPageAsync(PageType type, int organizationId)
        {
            _validator.CheckIfPageTypeExists(type);

            return (await _filterPresetDbSet
                .Where(preset => preset.OrganizationId == organizationId &&
                                 preset.ForPage == type)
                .ToListAsync())
                .Select(preset => new FilterPresetDto
                {
                    Id = preset.Id,
                    Name = preset.Name,
                    IsDefault = preset.IsDefault,
                    Filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset)
                });
        }

        public async Task<UpdatedFilterPresetDto> UpdateAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            await _filterPresetUpdateCreateLock.WaitAsync();

            try
            {
                _validator.CheckIfMoreThanOneDefaultPresetExists(updateDto);
                _validator.CheckIfFilterPresetsContainsUniqueNames(updateDto.PresetsToUpdate);
                _validator.CheckIfFilterPresetsContainsUniqueNames(updateDto.PresetsToCreate);
                
                await UpdatePresetsAsync(updateDto);
                
                var deletedPresets = await DeleteAsync(updateDto);

                // Need to call this after deletion, because we receive preset Ids that need to be deleted and not presets themselves
                await _validator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(updateDto, deletedPresets);

                var createdPresets = CreatePresets(updateDto);

                if (NewDefaultPresetWasSet(createdPresets, updateDto.PresetsToUpdate))
                {
                    await ChangeCurrentDefaultFilterToNonDefaultAsync(updateDto.PageType, updateDto.UserOrg.OrganizationId);
                }

                await _uow.SaveChangesAsync(updateDto.UserOrg.UserId);
                
                return new UpdatedFilterPresetDto
                {
                    // Ids are assigned after SaveChangesAsync()
                    CreatedPresets = createdPresets.Select(preset => new FilterPresetDto
                    {
                        Id = preset.Id,
                        Name = preset.Name,
                        IsDefault = preset.IsDefault,
                        Filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset)
                    }),
                    UpdatedPresets = updateDto.PresetsToUpdate,
                    DeletedPresets = deletedPresets
                };
            }
            finally
            {
                _filterPresetUpdateCreateLock.Release();
            }
        }

        public async Task<IEnumerable<FiltersDto>> GetFiltersAsync(FilterType[] filterTypes, int organizationId)
        {
            _validator.CheckIfFilterTypesContainsDuplicates(filterTypes);
            _validator.CheckIfFilterTypesAreValid(filterTypes);

            var filtersDtos = new List<FiltersDto>();

            foreach (var type in filterTypes)
            {
                var filters = await GetFiltersQueryByFilterType(type).ToListAsync();
                var filtersDto = new FiltersDto
                {
                    FilterType = type,
                    Filters = filters,
                };

                filtersDtos.Add(filtersDto);
            }

            return filtersDtos;
        }

        private bool NewDefaultPresetWasSet(IEnumerable<FilterPreset> createdPresets, IEnumerable<FilterPresetDto> updatedPresets)
        {
            return createdPresets.Any(preset => preset.IsDefault) || updatedPresets.Any(preset => preset.IsDefault);
        }

        private IQueryable<FilterDto> GetFiltersQueryByFilterType(FilterType filterType)
        {
            return filterType switch
            {
                FilterType.Offices => _officeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                FilterType.Kudos => _kudosTypeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                FilterType.Events => _eventTypeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                _ => throw new NotImplementedException()
            };
        }

        private async Task<IEnumerable<FilterPresetDto>> DeleteAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToDelete.Any())
            {
                return Enumerable.Empty<FilterPresetDto>();
            }

            var presets = await _filterPresetDbSet
                .Where(preset => 
                    updateDto.PresetsToDelete.Contains(preset.Id) &&
                    preset.OrganizationId == updateDto.UserOrg.OrganizationId)
                .ToListAsync();

            var removedPresets = presets.Select(preset => new FilterPresetDto
            {
                Id = preset.Id,
                Name = preset.Name,
                IsDefault = preset.IsDefault,
                Filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset)
            });

            _filterPresetDbSet.RemoveRange(presets);

            return removedPresets;
        }

        private IEnumerable<FilterPreset> CreatePresets(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToCreate.Any())
            {
                return Enumerable.Empty<FilterPreset>();
            }

            var timestamp = DateTime.UtcNow;
            var createdPresets = new List<FilterPreset>();

            foreach (var presetDtoToAdd in updateDto.PresetsToCreate)
            {
                var presetToAdd = new FilterPreset
                {
                    OrganizationId = updateDto.UserOrg.OrganizationId,
                    CreatedBy = updateDto.UserOrg.UserId,
                    Name = presetDtoToAdd.Name,
                    IsDefault = presetDtoToAdd.IsDefault,
                    ForPage = updateDto.PageType,
                    Modified = timestamp,
                    Created = timestamp,
                    Preset = JsonConvert.SerializeObject(presetDtoToAdd.Filters)
                };

                // AddRange() does not return assigned Id's after SaveChangesAsync(), but using Add() does
                _filterPresetDbSet.Add(presetToAdd);

                createdPresets.Add(presetToAdd);
            }

            return createdPresets;
        }

        private async Task UpdatePresetsAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToUpdate.Any())
            {
                return;
            }

            var presetsToUpdateIds = updateDto.PresetsToUpdate.Select(preset => preset.Id)
                .ToList();

            var existingPresets = await _filterPresetDbSet
                .Where(preset => presetsToUpdateIds.Contains(preset.Id))
                .ToDictionaryAsync(preset => preset.Id, preset => preset);

            _validator.CheckIfCountsAreEqual(presetsToUpdateIds, existingPresets);

            foreach (var preset in updateDto.PresetsToUpdate)
            {
                if (!existingPresets.TryGetValue(preset.Id, out var presetToUpdate))
                {
                    continue;
                }

                presetToUpdate.IsDefault = preset.IsDefault;
                presetToUpdate.Name = preset.Name;
                presetToUpdate.Preset = JsonConvert.SerializeObject(preset.Filters);
            }
        }

        private async Task ChangeCurrentDefaultFilterToNonDefaultAsync(PageType pageType, int organizationId)
        {
            // Find current default filter
            var defaultFilter = await _filterPresetDbSet
                .FirstOrDefaultAsync(filter => filter.IsDefault && 
                                     filter.ForPage == pageType &&
                                     filter.OrganizationId == organizationId);

            // Change to non default
            if (defaultFilter != null)
            {
                defaultFilter.IsDefault = false;
            }
        }
    }
}
