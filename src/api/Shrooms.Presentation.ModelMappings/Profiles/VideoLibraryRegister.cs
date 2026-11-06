using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="VideoLibrary"/> AutoMapper profile.</summary>
    public class VideoLibraryRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<VideoTypeDto, VideoTypeViewModel>();
            config.NewConfig<VideoLibraryItemDto, VideoLibraryItemViewModel>();
            config.NewConfig<VideoLibraryFiltersDto, VideoLibraryFiltersViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<VideoTypeViewModel, VideoTypeDto>();
            config.NewConfig<NewVideoTypeViewModel, VideoTypeDto>();
            config.NewConfig<VideoLibraryItemViewModel, VideoLibraryItemDto>();
            config.NewConfig<NewVideoLibraryItemViewModel, VideoLibraryItemDto>();
            config.NewConfig<VideoLibraryListArgsViewModel, VideoLibraryListArgsDto>();
            config.NewConfig<VideoLibraryFiltersArgsViewModel, VideoLibraryFiltersArgsDto>();
        }
    }
}
