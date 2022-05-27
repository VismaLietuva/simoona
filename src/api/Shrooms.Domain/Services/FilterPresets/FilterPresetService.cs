using Newtonsoft.Json;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;
using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.FilterPresets
{
    public class FilterPresetService : IFilterPresetService
    {
        private static readonly SemaphoreSlim _filterPresetUpdateCreateLock = new SemaphoreSlim(1, 1);

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<FilterPreset> _filterPresetDbSet;
        private readonly IFilterPresetValidator _validator;

        public FilterPresetService(IUnitOfWork2 uow, IFilterPresetValidator validator)
        {
            _uow = uow;
            _validator = validator;

            _filterPresetDbSet = uow.GetDbSet<FilterPreset>();
        }

        public async Task UpdateAsync(EditFilterPresetDto editDto, UserAndOrganizationDto userOrg)
        {
            await _filterPresetUpdateCreateLock.WaitAsync();
            
            try
            {
                _validator.CheckIfFilterPresetItemsContainDuplicates(editDto);

                await _validator.CheckIfFilterItemsExistsAsync(editDto);

                var filterPreset = await _filterPresetDbSet
                    .FirstAsync(filter => filter.Id == editDto.Id);

                if (filterPreset.Name != editDto.Name)
                {
                    await _validator.CheckIfFilterPresetExistsAsync(editDto);
                }

                if (editDto.IsDefault)
                {
                    await ChangeCurrentDefaultFilterToNonDefaultAsync(filterPreset);
                }

                filterPreset.Name = editDto.Name;
                filterPreset.ForPage = editDto.Type;
                filterPreset.Preset = JsonConvert.SerializeObject(editDto);

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _filterPresetUpdateCreateLock.Release();
            }
        }

        public async Task CreateAsync(CreateFilterPresetDto createDto, UserAndOrganizationDto userOrg)
        {
            await _filterPresetUpdateCreateLock.WaitAsync();

            try
            {
                _validator.CheckIfFilterPresetItemsContainDuplicates(createDto);

                await _validator.CheckIfFilterPresetExistsAsync(createDto);
                await _validator.CheckIfFilterItemsExistsAsync(createDto);

                var filterPreset = MapFilterPresetDtoToFilterPreset(createDto, userOrg);

                if (filterPreset.IsDefault)
                {
                    await ChangeCurrentDefaultFilterToNonDefaultAsync(filterPreset);
                }

                _filterPresetDbSet.Add(filterPreset);

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _filterPresetUpdateCreateLock.Release();
            }
        }

        private async Task ChangeCurrentDefaultFilterToNonDefaultAsync(FilterPreset preset)
        {
            // Find current default filter
            var defaultFilter = await _filterPresetDbSet
                .FirstOrDefaultAsync(filter => filter.IsDefault && filter.ForPage == preset.ForPage);

            // Change to non default
            if (defaultFilter != null)
            {
                defaultFilter.IsDefault = false;
            }
        }

        private static FilterPreset MapFilterPresetDtoToFilterPreset(FilterPresetDto createDto, UserAndOrganizationDto userOrg)
        {
            return new FilterPreset
            {
                Name = createDto.Name,
                IsDefault = createDto.IsDefault,
                ForPage = createDto.Type,
                Preset = JsonConvert.SerializeObject(createDto),
                OrganizationId = userOrg.OrganizationId
            };
        }
    }
}
