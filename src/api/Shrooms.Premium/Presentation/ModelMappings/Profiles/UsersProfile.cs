using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.User;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class UsersProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<EventUserSearchResultDto, EventUserSearchResultViewModel>();
        }
    }
}
