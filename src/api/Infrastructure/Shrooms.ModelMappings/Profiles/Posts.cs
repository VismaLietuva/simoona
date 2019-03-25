using AutoMapper;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.ModelMappings.Profiles
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
