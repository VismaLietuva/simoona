using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;
using Shrooms.Presentation.WebViewModels.Models.Monitors;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Monitors"/> AutoMapper profile.</summary>
    public class MonitorsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateMonitorViewModel, MonitorDto>()
                .Ignore(x => x.Id);
            config.NewConfig<MonitorViewModel, MonitorDto>();
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<MonitorDto, MonitorViewModel>();
        }
    }
}
