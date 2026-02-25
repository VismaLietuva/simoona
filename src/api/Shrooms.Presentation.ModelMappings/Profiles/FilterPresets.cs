using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Presentation.WebViewModels.Models.FilterPresets;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class FilterPresets : Profile
    {
        public FilterPresets()
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
            CreateMap<ManageFilterPresetViewModel, ManageFilterPresetDto>()
                .Ignore(opt => opt.UserOrg);
            CreateMap<FilterViewModel, FilterDto>();
            CreateMap<FiltersViewModel, FiltersDto>();
        }
    }
}
