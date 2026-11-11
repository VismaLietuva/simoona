using Mapster;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.User;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="UsersProfile"/> AutoMapper profile.</summary>
    public class UsersRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<EventUserSearchResultDto, EventUserSearchResultViewModel>();
        }
    }
}
