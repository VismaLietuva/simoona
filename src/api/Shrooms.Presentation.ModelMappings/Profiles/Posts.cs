using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Posts : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<NewlyCreatedPostDto, WallPostViewModel>();
            CreateMap<NewPostDto, WallPostViewModel>();
            CreateMap<PostDto, WallPostViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateWallPostViewModel, NewPostDto>();
            CreateMap<EditPostViewModel, EditPostDto>();
        }
    }
}
