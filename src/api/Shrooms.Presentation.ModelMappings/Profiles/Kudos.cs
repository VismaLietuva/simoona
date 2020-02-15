using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Presentation.WebViewModels.Models.KudosTypes;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Kudos : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<KudosLogUserDTO, KudosLogUserViewModel>();
            CreateMap<KudosLogTypeDTO, KudosLogTypeViewModel>();
            CreateMap<KudosUserLogDTO, KudosUserLogViewModel>();
            CreateMap<MainKudosLogDTO, KudosLogViewModel>();
            CreateMap<WallKudosLogDTO, WallKudosLogViewModel>();
            CreateMap<KudosBasicDataDTO, KudosBasicDataViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddKudosLogViewModel, AddKudosLogDTO>();
            CreateMap<KudosLogsFilterViewModel, KudosLogsFilterDTO>();
            CreateMap<KudosRejectViewModel, KudosRejectDTO>();
            CreateMap<KudosBasicDataViewModel, KudosBasicDataDTO>();
            CreateMap<NewKudosTypeViewModel, NewKudosTypeDto>();
            CreateMap<KudosTypeViewModel, KudosTypeDTO>();
        }
    }
}