using Mapster;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="VacationsProfile"/> AutoMapper profile.</summary>
    public class VacationsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<VacationDto, VacationViewModel>()
                .Map(dest => dest.DateStart, src => src.DateFrom)
                .Map(dest => dest.DateEnd, src => src.DateTo);

            config.NewConfig<VacationAvailableDaysDto, VacationAvailableDaysViewModel>();
        }
    }
}
