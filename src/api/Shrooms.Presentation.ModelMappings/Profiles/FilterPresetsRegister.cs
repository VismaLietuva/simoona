using Mapster;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Presentation.WebViewModels.Models.FilterPresets;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="FilterPresets"/> AutoMapper profile.</summary>
    public class FilterPresetsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<FilterPresetItemDto, FilterPresetItemViewModel>();
            config.NewConfig<FilterPresetDto, FilterPresetViewModel>()
                .Ignore(x => x.PageType);
            config.NewConfig<FilterDto, FilterViewModel>();
            config.NewConfig<FiltersDto, FiltersViewModel>();
            config.NewConfig<UpdatedFilterPresetDto, UpdatedFilterPresetViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateFilterPresetViewModel, CreateFilterPresetDto>()
                .Ignore(x => x.Id);
            config.NewConfig<CreateFilterPresetViewModel, FilterPresetDto>()
                .Ignore(x => x.Id);
            config.NewConfig<FilterPresetItemViewModel, FilterPresetItemDto>();
            config.NewConfig<UpdateFilterPresetViewModel, UpdateFilterPresetDto>();
            config.NewConfig<UpdateFilterPresetViewModel, FilterPresetDto>();
            config.NewConfig<ManageFilterPresetViewModel, ManageFilterPresetDto>()
                .Ignore(x => x.UserOrg);
            config.NewConfig<FilterViewModel, FilterDto>();
            config.NewConfig<FiltersViewModel, FiltersDto>();
        }
    }
}
