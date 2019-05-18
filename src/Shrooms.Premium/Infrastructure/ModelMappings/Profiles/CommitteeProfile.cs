using AutoMapper;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Committee;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Committees;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
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
