using Mapster;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Presentation.WebViewModels.KudosShop;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="KudosShopProfile"/> AutoMapper profile.</summary>
    public class KudosShopRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<KudosShopItemDto, KudosShopItemViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<KudosShopItemViewModel, KudosShopItemDto>().IgnoreUserOrgDto();
        }
    }
}
