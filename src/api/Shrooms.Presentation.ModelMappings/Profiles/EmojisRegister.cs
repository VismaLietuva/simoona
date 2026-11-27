using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Presentation.WebViewModels.Models.Emoji;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Emojis"/> AutoMapper profile.</summary>
    public class EmojisRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CustomEmojiDto, CustomEmojiViewModel>();
        }
    }
}
