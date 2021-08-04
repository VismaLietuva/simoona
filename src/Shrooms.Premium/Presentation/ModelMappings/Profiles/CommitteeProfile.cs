using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Presentation.WebViewModels.Committees;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class CommitteeProfile : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateViewModelMappings();
            CreateDtoMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CommitteePostViewModel, CommitteePostDto>();
            CreateMap<CommitteeSuggestionPostViewModel, CommitteeSuggestionPostDto>();
        }

        private void CreateViewModelMappings()
        {
            CreateMap<CommitteeViewModel, Committee>().ReverseMap();
            CreateMap<CommitteePostViewModel, Committee>().ReverseMap();

            CreateMap<CommitteeSuggestionDto, CommitteeSuggestionViewModel>();
        }

        private void CreateDtoMappings()
        {
            CreateMap<CommitteePostDto, Committee>()
              .ForMember(dest => dest.Members, src => src.Ignore());
            CreateMap<CommitteeSuggestionPostDto, CommitteeSuggestion>();

            CreateMap<Committee, CommitteeViewDto>();
            CreateMap<ApplicationUser, CommitteeMembersDto>();
            CreateMap<CommitteeSuggestion, CommitteeSuggestionDto>();
        }
    }
}
