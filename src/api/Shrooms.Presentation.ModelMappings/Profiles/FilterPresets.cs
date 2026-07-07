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
            CreateMap<FilterPresetItemDto, FilterPresetItemViewModel>(MemberList.None);
            CreateMap<FilterPresetDto, FilterPresetViewModel>(MemberList.None)
                .Ignore(opt => opt.PageType);
            CreateMap<FilterDto, FilterViewModel>(MemberList.None);
            CreateMap<FiltersDto, FiltersViewModel>(MemberList.None);
            CreateMap<UpdatedFilterPresetDto, UpdatedFilterPresetViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateFilterPresetViewModel, CreateFilterPresetDto>(MemberList.None)
                .Ignore(opt => opt.Id);
            CreateMap<CreateFilterPresetViewModel, FilterPresetDto>(MemberList.None)
                .Ignore(opt => opt.Id);
            CreateMap<FilterPresetItemViewModel, FilterPresetItemDto>(MemberList.None);
            CreateMap<UpdateFilterPresetViewModel, UpdateFilterPresetDto>(MemberList.None);
            CreateMap<UpdateFilterPresetViewModel, FilterPresetDto>(MemberList.None);
            CreateMap<ManageFilterPresetViewModel, ManageFilterPresetDto>(MemberList.None)
                .Ignore(opt => opt.UserOrg);
            CreateMap<FilterViewModel, FilterDto>(MemberList.None);
            CreateMap<FiltersViewModel, FiltersDto>(MemberList.None);
        }
    }
}
