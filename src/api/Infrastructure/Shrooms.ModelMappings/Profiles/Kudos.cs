using AutoMapper;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.WebViewModels.Models.Kudos;
using Shrooms.WebViewModels.Models.KudosTypes;

namespace Shrooms.ModelMappings.Profiles
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