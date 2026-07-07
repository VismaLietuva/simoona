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
            CreateMap<KudosBasketLogDto, KudosBasketLogViewModel>(MemberList.None);
            CreateMap<KudosBasketLogUserDto, KudosBasketLogUserViewModel>(MemberList.None);
            CreateMap<KudosBasketDto, KudosBasketViewModel>(MemberList.None);
            CreateMap<KudosBasketDto, KudosBasketWidgetViewModel>(MemberList.None);
            CreateMap<KudosBasketCreateDto, KudosBasketCreateViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<KudosBasketCreateViewModel, KudosBasketCreateDto>(MemberList.None);
            CreateMap<KudosBasketEditViewModel, KudosBasketEditDto>(MemberList.None);
            CreateMap<KudosBasketDonateViewModel, KudosBasketDonationDto>(MemberList.None);
        }
    }
}
