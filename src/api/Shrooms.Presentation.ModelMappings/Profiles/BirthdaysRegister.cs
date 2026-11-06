using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Birthdays;
using Shrooms.Presentation.WebViewModels.Models.Birthday;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Birthdays"/> AutoMapper profile.</summary>
    public class BirthdaysRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<BirthdayDto, BirthdayViewModel>();
        }
    }
}
