using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Likes : Profile
    {
        public Likes()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<LikeDto, LikeViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddLikeViewModel, AddLikeDto>(MemberList.None);
        }
    }
}