using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments;
using System.Collections.Generic;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Comments : Profile
    {
        public Comments()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEntityToViewModel();
            CreateViewModelToEntity();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<CommentDto, CommentViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<EditCommentViewModel, EditCommentDto>(MemberList.None)
                .IgnoreUserOrgDto()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));
            CreateMap<NewCommentViewModel, NewCommentDto>(MemberList.None)
                .IgnoreUserOrgDto()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));
            CreateMap<CommentPostViewModel, EditCommentDto>(MemberList.None)
                .IgnoreUserOrgDto();
            CreateMap<CommentPostViewModel, NewCommentDto>(MemberList.None)
                .IgnoreUserOrgDto();
        }

        private void CreateEntityToViewModel()
        {
            CreateMap<Comment, Presentation.WebViewModels.Models.CommentViewModel>(MemberList.None);
            CreateMap<Comment, CommentPostViewModel>(MemberList.None);
        }

        private void CreateViewModelToEntity()
        {
            CreateMap<Presentation.WebViewModels.Models.CommentViewModel, Comment>(MemberList.None);
            CreateMap<CommentPostViewModel, Comment>(MemberList.None);
        }
    }
}
