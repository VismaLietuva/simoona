using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class VideoLibrary : Profile
    {
        public VideoLibrary()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<VideoTypeDto, VideoTypeViewModel>(MemberList.None);
            CreateMap<VideoLibraryItemDto, VideoLibraryItemViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<VideoTypeViewModel, VideoTypeDto>(MemberList.None);
            CreateMap<NewVideoTypeViewModel, VideoTypeDto>(MemberList.None);
            CreateMap<VideoLibraryItemViewModel, VideoLibraryItemDto>(MemberList.None);
            CreateMap<NewVideoLibraryItemViewModel, VideoLibraryItemDto>(MemberList.None);
        }
    }
}
