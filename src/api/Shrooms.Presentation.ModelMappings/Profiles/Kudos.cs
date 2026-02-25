using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Presentation.WebViewModels.Models.KudosTypes;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Kudos : Profile
    {
        public Kudos()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosLogUserDto, KudosLogUserViewModel>();
            CreateMap<KudosLogTypeDto, KudosLogTypeViewModel>();
            CreateMap<KudosUserLogDto, KudosUserLogViewModel>();
            CreateMap<MainKudosLogDto, KudosLogViewModel>();
            CreateMap<WallKudosLogDto, WallKudosLogViewModel>();
            CreateMap<KudosBasicDataDto, KudosBasicDataViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddKudosLogViewModel, AddKudosLogDto>();
            CreateMap<KudosLogsFilterViewModel, KudosLogsFilterDto>();
            CreateMap<KudosRejectViewModel, KudosRejectDto>();
            CreateMap<KudosBasicDataViewModel, KudosBasicDataDto>();
            CreateMap<NewKudosTypeViewModel, NewKudosTypeDto>();
            CreateMap<KudosTypeViewModel, KudosTypeDto>();
        }
    }
}