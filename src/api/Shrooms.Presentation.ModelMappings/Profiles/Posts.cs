using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts;
using System.Collections.Generic;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Posts : Profile
    {
        public Posts()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<NewlyCreatedPostDto, WallPostViewModel>(MemberList.None);
            CreateMap<NewPostDto, WallPostViewModel>(MemberList.None);
            CreateMap<PostDto, WallPostViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateWallPostViewModel, NewPostDto>(MemberList.None)
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));

            CreateMap<EditPostViewModel, EditPostDto>(MemberList.None)
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));
        }
    }
}
