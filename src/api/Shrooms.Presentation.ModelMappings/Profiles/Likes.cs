using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Likes : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<LikeDto, LikeViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddLikeViewModel, AddLikeDto>();
        }
    }
}