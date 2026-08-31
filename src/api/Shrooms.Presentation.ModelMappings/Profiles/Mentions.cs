using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;
using Shrooms.Presentation.WebViewModels.Models.Wall.Mentions;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Mentions : Profile
    {
        public Mentions()
        {
            CreateMap<MentionPersonDto, MentionPersonViewModel>(MemberList.None);
            CreateMap<MentionGroupDto, MentionGroupViewModel>(MemberList.None);
            CreateMap<MentionSuggestionsDto, MentionSuggestionsViewModel>(MemberList.None);
        }
    }
}
