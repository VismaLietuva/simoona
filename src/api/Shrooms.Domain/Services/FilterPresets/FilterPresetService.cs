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
        private static readonly SemaphoreSlim _lock = new (1, 1);

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

        public async Task<UpdatedFilterPresetDto> UpdateAsync(ManageFilterPresetDto manageFilterPresetDto)
        {
            await _lock.WaitAsync();

            try
            {
                _validator.CheckIfMoreThanOneDefaultPresetExists(manageFilterPresetDto);
                _validator.CheckIfFilterPresetsContainUniqueNames(manageFilterPresetDto.PresetsToUpdate);
                _validator.CheckIfFilterPresetsContainUniqueNames(manageFilterPresetDto.PresetsToCreate);

                await _validator.CheckIfProvidedTypesInFiltersAreValidAsync(manageFilterPresetDto);

                await UpdatePresetsAsync(manageFilterPresetDto);

                var deletedPresets = await DeleteAsync(manageFilterPresetDto);

                // Need to call this after deletion, because we receive preset Ids that need to be deleted and not presets themselves
                await _validator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageFilterPresetDto, deletedPresets);

                var createdPresets = CreatePresets(manageFilterPresetDto);

                if (IsNewDefaultPresetSet(createdPresets, manageFilterPresetDto.PresetsToUpdate))
                {
                    await ChangeCurrentDefaultFilterToNonDefaultAsync(manageFilterPresetDto.PageType, manageFilterPresetDto.UserOrg.OrganizationId);
                }

                await _uow.SaveChangesAsync(manageFilterPresetDto.UserOrg.UserId);

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
                    UpdatedPresets = manageFilterPresetDto.PresetsToUpdate,
                    DeletedPresets = deletedPresets
                };
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveDeletedTypeFromPresetsAsync(string deletedTypeId, FilterType type, int organizationId)
        {
            await _lock.WaitAsync();

            try
            {
                var presets = await _filterPresetDbSet.Where(preset =>
                    preset.OrganizationId == organizationId || type == FilterType.Kudos) // Kudos type does not have organization
                    .ToListAsync();

                foreach (var preset in presets)
                {
                    var filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset);

                    RemoveDeletedTypeFromFilters(filters, deletedTypeId, type);

                    preset.Preset = JsonConvert.SerializeObject(filters);
                }

                await _uow.SaveChangesAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IEnumerable<FiltersDto>> GetFiltersAsync(FilterType[] filterTypes, int organizationId)
        {
            _validator.CheckIfFilterTypesContainDuplicates(filterTypes);
            _validator.CheckIfFilterTypesAreValid(filterTypes);

            var filtersDtos = new List<FiltersDto>();

            foreach (var type in filterTypes)
            {
                var filters = await GetFiltersQueryByFilterType(type, organizationId).ToListAsync();
                var filtersDto = new FiltersDto
                {
                    FilterType = type,
                    Filters = filters
                };

                filtersDtos.Add(filtersDto);
            }

            return filtersDtos;
        }

        private void RemoveDeletedTypeFromFilters(IEnumerable<FilterPresetItemDto> filters, string typeId, FilterType type)
        {
            foreach (var filter in filters)
            {
                if (filter.FilterType != type)
                {
                    continue;
                }

                filter.Types = filter.Types.Where(t => t != typeId);
            }
        }

        private bool IsNewDefaultPresetSet(IEnumerable<FilterPreset> createdPresets, IEnumerable<FilterPresetDto> updatedPresets)
        {
            return createdPresets.Any(preset => preset.IsDefault) || updatedPresets.Any(preset => preset.IsDefault);
        }

        private IQueryable<FilterDto> GetFiltersQueryByFilterType(FilterType filterType, int organizationId)
        {
            return filterType switch
            {
                FilterType.Offices => _officeDbSet.Where(office => office.OrganizationId == organizationId)
                .Select(office => new FilterDto
                {
                    Id = office.Id,
                    Name = office.Name
                }),
                FilterType.Kudos => _kudosTypeDbSet.Select(kudos => new FilterDto // Does not have organization
                {
                    Id = kudos.Id,
                    Name = kudos.Name
                }),
                FilterType.Events => _eventTypeDbSet.Where(@event => @event.OrganizationId == organizationId)
                .Select(@event => new FilterDto
                {
                    Id = @event.Id,
                    Name = @event.Name
                }),
                _ => throw new NotImplementedException()
            };
        }

        private async Task<IList<FilterPresetDto>> DeleteAsync(ManageFilterPresetDto manageFilterPresetDto)
        {
            if (!manageFilterPresetDto.PresetsToDelete.Any())
            {
                return new List<FilterPresetDto>();
            }

            var presets = await _filterPresetDbSet
                .Where(preset =>
                    manageFilterPresetDto.PresetsToDelete.Contains(preset.Id) &&
                    preset.OrganizationId == manageFilterPresetDto.UserOrg.OrganizationId)
                .ToListAsync();

            var removedPresets = presets.Select(preset => new FilterPresetDto
            {
                Id = preset.Id,
                Name = preset.Name,
                IsDefault = preset.IsDefault,
                Filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset)
            }).ToList();

            _filterPresetDbSet.RemoveRange(presets);

            return removedPresets;
        }

        private IList<FilterPreset> CreatePresets(ManageFilterPresetDto manageFilterPresetDto)
        {
            if (!manageFilterPresetDto.PresetsToCreate.Any())
            {
                return new List<FilterPreset>();
            }

            var timestamp = DateTime.UtcNow;
            var createdPresets = new List<FilterPreset>();

            foreach (var presetDtoToAdd in manageFilterPresetDto.PresetsToCreate)
            {
                var presetToAdd = new FilterPreset
                {
                    OrganizationId = manageFilterPresetDto.UserOrg.OrganizationId,
                    CreatedBy = manageFilterPresetDto.UserOrg.UserId,
                    Name = presetDtoToAdd.Name,
                    IsDefault = presetDtoToAdd.IsDefault,
                    ForPage = manageFilterPresetDto.PageType,
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

        private async Task UpdatePresetsAsync(ManageFilterPresetDto manageFilterPresetDto)
        {
            if (!manageFilterPresetDto.PresetsToUpdate.Any())
            {
                return;
            }

            var presetsToUpdateIds = manageFilterPresetDto.PresetsToUpdate.Select(preset => preset.Id)
                .ToList();

            var existingPresets = await _filterPresetDbSet
                .Where(preset => presetsToUpdateIds.Contains(preset.Id))
                .ToDictionaryAsync(preset => preset.Id, preset => preset);

            _validator.CheckIfCountsAreEqual(presetsToUpdateIds, existingPresets);

            foreach (var preset in manageFilterPresetDto.PresetsToUpdate)
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
