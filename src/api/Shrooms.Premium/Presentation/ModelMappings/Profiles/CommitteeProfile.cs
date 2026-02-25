using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Presentation.WebViewModels.Committees;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class CommitteeProfile : Profile
    {
        public CommitteeProfile()
        {
            CreateViewModelToDtoMappings();
            CreateViewModelMappings();
            CreateDtoMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CommitteePostViewModel, CommitteePostDto>(MemberList.None);
            CreateMap<CommitteeSuggestionPostViewModel, CommitteeSuggestionPostDto>(MemberList.None);
        }

        private void CreateViewModelMappings()
        {
            CreateMap<CommitteeViewModel, Committee>(MemberList.None).ReverseMap();
            CreateMap<CommitteePostViewModel, Committee>(MemberList.None).ReverseMap();

            CreateMap<CommitteeSuggestionDto, CommitteeSuggestionViewModel>(MemberList.None);
        }

        private void CreateDtoMappings()
        {
            CreateMap<CommitteePostDto, Committee>(MemberList.None)
              .ForMember(dest => dest.Members, src => src.Ignore());
            CreateMap<CommitteeSuggestionPostDto, CommitteeSuggestion>(MemberList.None);

            CreateMap<Committee, CommitteeViewDto>(MemberList.None);
            CreateMap<ApplicationUser, CommitteeMembersDto>(MemberList.None);
            CreateMap<CommitteeSuggestion, CommitteeSuggestionDto>(MemberList.None);
        }
    }
}
