using Mapster;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models.VacationPage;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="VacationPages"/> AutoMapper profile.</summary>
    public class VacationPagesRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
            CreateEntityToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<VacationPageDto, VacationPageViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<VacationPageViewModel, VacationPageDto>();
        }

        private static void CreateEntityToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<VacationPage, VacationPageDto>();
        }
    }
}
