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
            CreateMap<KudosLogUserDto, KudosLogUserViewModel>(MemberList.None);
            CreateMap<KudosLogTypeDto, KudosLogTypeViewModel>(MemberList.None);
            CreateMap<KudosUserLogDto, KudosUserLogViewModel>(MemberList.None);
            CreateMap<MainKudosLogDto, KudosLogViewModel>(MemberList.None);
            CreateMap<WallKudosLogDto, WallKudosLogViewModel>(MemberList.None);
            CreateMap<KudosBasicDataDto, KudosBasicDataViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddKudosLogViewModel, AddKudosLogDto>(MemberList.None);
            CreateMap<KudosLogsFilterViewModel, KudosLogsFilterDto>(MemberList.None);
            CreateMap<KudosRejectViewModel, KudosRejectDto>(MemberList.None);
            CreateMap<KudosBasicDataViewModel, KudosBasicDataDto>(MemberList.None);
            CreateMap<NewKudosTypeViewModel, NewKudosTypeDto>(MemberList.None);
            CreateMap<KudosTypeViewModel, KudosTypeDto>(MemberList.None);
        }
    }
}