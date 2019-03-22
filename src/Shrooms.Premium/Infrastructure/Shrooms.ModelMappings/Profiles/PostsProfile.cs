using AutoMapper;
using Shrooms.WebViewModels.Models.Events;
using Shrooms.WebViewModels.Models.Wall.Posts;
using Shrooms.DataTransferObjects.Models.Wall.Posts;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
{
    public class PostsProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<NewlyCreatedPostDTO, EventPostViewModel>();
            CreateMap<PostDTO, EventPostViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ShareEventViewModel, NewPostDTO>()
                .ForMember(dest => dest.SharedEventId, opt => opt.MapFrom(u => u.EventId));
        }
    }
}
