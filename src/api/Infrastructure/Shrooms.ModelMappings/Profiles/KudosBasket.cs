using AutoMapper;
using Shrooms.DataTransferObjects.Models.KudosBasket;
using Shrooms.WebViewModels.Models.KudosBaskets;
using Shrooms.WebViewModels.Models.Wall.Widgets;

namespace Shrooms.ModelMappings.Profiles
{
    public class KudosBasket : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosBasketLogDTO, KudosBasketLogViewModel>();
            CreateMap<KudosBasketLogUserDTO, KudosBasketLogUserViewModel>();
            CreateMap<KudosBasketDTO, KudosBasketViewModel>();
            CreateMap<KudosBasketDTO, KudosBasketWidgetViewModel>();
            CreateMap<KudosBasketCreateDTO, KudosBasketCreateViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<KudosBasketCreateViewModel, KudosBasketCreateDTO>();
            CreateMap<KudosBasketEditViewModel, KudosBasketEditDTO>();
            CreateMap<KudosBasketDonateViewModel, KudosBasketDonationDTO>();
        }
    }
}
