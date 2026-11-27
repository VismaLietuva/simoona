using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Presentation.WebViewModels.Models.KudosBaskets;
using Shrooms.Presentation.WebViewModels.Models.Wall.Widgets;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="KudosBasket"/> AutoMapper profile.</summary>
    public class KudosBasketRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<KudosBasketLogDto, KudosBasketLogViewModel>();
            config.NewConfig<KudosBasketLogUserDto, KudosBasketLogUserViewModel>();
            config.NewConfig<KudosBasketDto, KudosBasketViewModel>();
            config.NewConfig<KudosBasketDto, KudosBasketWidgetViewModel>();
            config.NewConfig<KudosBasketCreateDto, KudosBasketCreateViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<KudosBasketCreateViewModel, KudosBasketCreateDto>();
            config.NewConfig<KudosBasketEditViewModel, KudosBasketEditDto>();
            config.NewConfig<KudosBasketDonateViewModel, KudosBasketDonationDto>();
        }
    }
}
