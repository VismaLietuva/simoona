using Mapster;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;
using Shrooms.Presentation.WebViewModels.Models.Wall.Mentions;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Mentions"/> AutoMapper profile.</summary>
    public class MentionsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MentionPersonDto, MentionPersonViewModel>();
            config.NewConfig<MentionGroupDto, MentionGroupViewModel>();
            config.NewConfig<MentionSuggestionsDto, MentionSuggestionsViewModel>();
        }
    }
}
