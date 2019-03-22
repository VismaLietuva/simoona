using AutoMapper;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.WebViewModels.Models.KudosShop;

namespace Shrooms.ModelMappings.Profiles
{
    public class KudosShopProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosShopItemDTO, KudosShopItemViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<KudosShopItemViewModel, KudosShopItemDTO>().IgnoreUserOrgDto();
        }
    }
}
