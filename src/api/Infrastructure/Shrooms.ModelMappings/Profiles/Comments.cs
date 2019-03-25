using AutoMapper;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.WebViewModels.Models.PostModels;
using Shrooms.WebViewModels.Models.Wall.Posts.Comments;

namespace Shrooms.ModelMappings.Profiles
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
            CreateMap<Comment, WebViewModels.Models.CommentViewModel>();
            CreateMap<Comment, CommentPostViewModel>();
        }

        private void CreateViewModelToEntity()
        {
            CreateMap<WebViewModels.Models.CommentViewModel, Comment>();
            CreateMap<CommentPostViewModel, Comment>();
        }
    }
}
