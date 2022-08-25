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
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEntityToViewModel();
            CreateViewModelToEntity();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<CommentDto, CommentViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<EditCommentViewModel, EditCommentDto>()
                .IgnoreUserOrgDto()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));
            CreateMap<NewCommentViewModel, NewCommentDto>()
                .IgnoreUserOrgDto()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(u => u.PictureId != null ? new List<string> { u.PictureId } : u.Images));
            CreateMap<CommentPostViewModel, EditCommentDto>()
                .IgnoreUserOrgDto();
            CreateMap<CommentPostViewModel, NewCommentDto>()
                .IgnoreUserOrgDto();
        }

        private void CreateEntityToViewModel()
        {
            CreateMap<Comment, Presentation.WebViewModels.Models.CommentViewModel>();
            CreateMap<Comment, CommentPostViewModel>();
        }

        private void CreateViewModelToEntity()
        {
            CreateMap<Presentation.WebViewModels.Models.CommentViewModel, Comment>();
            CreateMap<CommentPostViewModel, Comment>();
        }
    }
}
