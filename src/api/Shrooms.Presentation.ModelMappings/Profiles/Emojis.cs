using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Presentation.WebViewModels.Models.Emoji;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Emojis : Profile
    {
        public Emojis()
        {
            CreateMap<CustomEmojiDto, CustomEmojiViewModel>(MemberList.None);
        }
    }
}
