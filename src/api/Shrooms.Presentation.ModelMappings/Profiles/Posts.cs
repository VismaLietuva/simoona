using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
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
            CreateMap<NewlyCreatedPostDTO, WallPostViewModel>();
            CreateMap<NewPostDTO, WallPostViewModel>();
            CreateMap<PostDTO, WallPostViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateWallPostViewModel, NewPostDTO>();
            CreateMap<EditPostViewModel, EditPostDTO>();
        }
    }
}
