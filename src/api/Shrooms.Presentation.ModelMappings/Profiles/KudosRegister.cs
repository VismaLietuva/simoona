using Mapster;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Presentation.WebViewModels.Models.KudosTypes;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Kudos"/> AutoMapper profile.</summary>
    public class KudosRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<KudosLogUserDto, KudosLogUserViewModel>();
            config.NewConfig<KudosLogTypeDto, KudosLogTypeViewModel>();
            config.NewConfig<KudosUserLogDto, KudosUserLogViewModel>();
            config.NewConfig<MainKudosLogDto, KudosLogViewModel>();
            config.NewConfig<WallKudosLogDto, WallKudosLogViewModel>();
            config.NewConfig<KudosBasicDataDto, KudosBasicDataViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<AddKudosLogViewModel, AddKudosLogDto>();
            config.NewConfig<KudosLogsFilterViewModel, KudosLogsFilterDto>();
            config.NewConfig<KudosRejectViewModel, KudosRejectDto>();
            config.NewConfig<KudosBasicDataViewModel, KudosBasicDataDto>();
            config.NewConfig<NewKudosTypeViewModel, NewKudosTypeDto>();
            config.NewConfig<KudosTypeViewModel, KudosTypeDto>();
        }
    }
}
