using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Presentation.WebViewModels.Models.KudosBaskets;
using Shrooms.Presentation.WebViewModels.Models.Wall.Widgets;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class KudosBasket : Profile
    {
        public KudosBasket()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosBasketLogDto, KudosBasketLogViewModel>();
            CreateMap<KudosBasketLogUserDto, KudosBasketLogUserViewModel>();
            CreateMap<KudosBasketDto, KudosBasketViewModel>();
            CreateMap<KudosBasketDto, KudosBasketWidgetViewModel>();
            CreateMap<KudosBasketCreateDto, KudosBasketCreateViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<KudosBasketCreateViewModel, KudosBasketCreateDto>();
            CreateMap<KudosBasketEditViewModel, KudosBasketEditDto>();
            CreateMap<KudosBasketDonateViewModel, KudosBasketDonationDto>();
        }
    }
}
