using AutoMapper;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.User;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
{
    public class UsersProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<EventUserSearchResultDTO, EventUserSearchResultViewModel>();
        }
    }
}
