using AutoMapper;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.KudosShop;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
