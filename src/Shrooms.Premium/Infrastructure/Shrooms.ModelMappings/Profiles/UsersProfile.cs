using AutoMapper;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.User;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
