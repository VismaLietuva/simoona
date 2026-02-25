using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Presentation.WebViewModels.Wall.Posts;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class PostsProfile : Profile
    {
        public PostsProfile()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<NewlyCreatedPostDto, EventPostViewModel>(MemberList.None);
            CreateMap<PostDto, EventPostViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ShareEventViewModel, NewPostDto>(MemberList.None)
                .ForMember(dest => dest.SharedEventId, opt => opt.MapFrom(u => u.Id))
                .ForMember(dest => dest.MentionedUserIds, opt => opt.MapFrom(_ => new List<string>()));
        }
    }
}
