using Mapster;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Likes"/> AutoMapper profile.</summary>
    public class LikesRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<LikeDto, LikeViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<AddLikeViewModel, AddLikeDto>();
        }
    }
}
