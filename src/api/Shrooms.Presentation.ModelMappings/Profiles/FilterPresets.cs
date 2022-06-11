using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Presentation.WebViewModels.Models.FilterPresets;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class FilterPresets : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<FilterPresetItemDto, FilterPresetItemViewModel>();
            CreateMap<FilterPresetDto, FilterPresetViewModel>();
            CreateMap<FilterDto, FilterViewModel>();
            CreateMap<FiltersDto, FiltersViewModel>();
            CreateMap<UpdatedFilterPresetDto, UpdatedFilterPresetViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateFilterPresetViewModel, CreateFilterPresetDto>();
            CreateMap<CreateFilterPresetViewModel, FilterPresetDto>();
            CreateMap<FilterPresetItemViewModel, FilterPresetItemDto>();
            CreateMap<EditFilterPresetViewModel, EditFilterPresetDto>();
            CreateMap<EditFilterPresetViewModel, FilterPresetDto>();
            CreateMap<AddEditDeleteFilterPresetViewModel, AddEditDeleteFilterPresetDto>();
            CreateMap<FilterViewModel, FilterDto>();
            CreateMap<FiltersViewModel, FiltersDto>();
        }
    }
}
