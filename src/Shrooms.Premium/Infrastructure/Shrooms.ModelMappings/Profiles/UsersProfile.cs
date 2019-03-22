using AutoMapper;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.WebViewModels.Models.User;

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
