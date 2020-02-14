using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Wall.Posts;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
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
