using AutoMapper;
using Shrooms.DataTransferObjects.Models.Committees;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models.Committees;

namespace Shrooms.ModelMappings.Profiles
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
            CreateMap<CommitteePostViewModel, CommitteePostDTO>();        
            CreateMap<CommitteeSuggestionPostViewModel, CommitteeSuggestionPostDTO>();                     
        }

        private void CreateViewModelMappings()
        {
            CreateMap<CommitteeViewModel, Committee>().ReverseMap();
            CreateMap<CommitteePostViewModel, Committee>().ReverseMap();

            CreateMap<CommitteeSuggestionViewDTO, CommitteeSuggestionViewModel>();
        }

        private void CreateDtoMappings()
        {
            CreateMap<CommitteePostDTO, Committee>()
              .ForMember(dest => dest.Members, src => src.Ignore());
            CreateMap<CommitteeSuggestionPostDTO, CommitteeSuggestion>();

            CreateMap<Committee, CommitteeViewDTO>();
            CreateMap<ApplicationUser, CommitteeMembersDTO>();
            CreateMap<CommitteeSuggestion, CommitteeSuggestionViewDTO>();
        }
    }
}
