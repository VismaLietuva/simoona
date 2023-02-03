using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Presentation.WebViewModels.Wall.Posts;
using System.Collections.Generic;

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
            CreateMap<NewlyCreatedPostDto, EventPostViewModel>();
            CreateMap<PostDto, EventPostViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ShareEventViewModel, NewPostDto>()
                .ForMember(dest => dest.SharedEventId, opt => opt.MapFrom(u => u.Id))
                .ForMember(dest => dest.MentionedUserIds, opt => opt.UseValue(new List<string>()));
        }
    }
}
