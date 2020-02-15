using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Presentation.WebViewModels.Wall.Posts;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
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
