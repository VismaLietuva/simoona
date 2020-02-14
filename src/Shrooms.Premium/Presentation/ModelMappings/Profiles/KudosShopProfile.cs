using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Presentation.WebViewModels.Models.KudosShop;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
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
