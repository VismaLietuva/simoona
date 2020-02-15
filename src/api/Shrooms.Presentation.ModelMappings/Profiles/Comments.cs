using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments;

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
            CreateMap<EditCommentViewModel, EditCommentDTO>()
                .IgnoreUserOrgDto();
            CreateMap<NewCommentViewModel, NewCommentDTO>()
                .IgnoreUserOrgDto();
            CreateMap<CommentPostViewModel, EditCommentDTO>()
                .IgnoreUserOrgDto();
            CreateMap<CommentPostViewModel, NewCommentDTO>()
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
