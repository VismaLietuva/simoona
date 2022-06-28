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
            CreateMap<FilterPresetDto, FilterPresetViewModel>()
                .Ignore(opt => opt.PageType);
            CreateMap<FilterDto, FilterViewModel>();
            CreateMap<FiltersDto, FiltersViewModel>();
            CreateMap<UpdatedFilterPresetDto, UpdatedFilterPresetViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateFilterPresetViewModel, CreateFilterPresetDto>()
                .Ignore(opt => opt.Id);
            CreateMap<CreateFilterPresetViewModel, FilterPresetDto>()
                .Ignore(opt => opt.Id);
            CreateMap<FilterPresetItemViewModel, FilterPresetItemDto>();
            CreateMap<UpdateFilterPresetViewModel, UpdateFilterPresetDto>();
            CreateMap<UpdateFilterPresetViewModel, FilterPresetDto>();
            CreateMap<AddEditDeleteFilterPresetViewModel, AddEditDeleteFilterPresetDto>()
                .Ignore(opt => opt.UserOrg)
                .Ignore(opt => opt.Name);
            CreateMap<FilterViewModel, FilterDto>();
            CreateMap<FiltersViewModel, FiltersDto>();
        }
    }
}
