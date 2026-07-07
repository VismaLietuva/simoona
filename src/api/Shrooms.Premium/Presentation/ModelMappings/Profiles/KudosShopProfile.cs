using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Presentation.WebViewModels.KudosShop;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class KudosShopProfile : Profile
    {
        public KudosShopProfile()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosShopItemDto, KudosShopItemViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<KudosShopItemViewModel, KudosShopItemDto>(MemberList.None).IgnoreUserOrgDto();
        }
    }
}
