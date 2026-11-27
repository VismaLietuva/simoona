using Mapster;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="LoyaltyKudosProfile"/> AutoMapper profile.</summary>
    public class LoyaltyKudosRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AwardedKudosEmployeeDto, KudosLog>();
            config.NewConfig<KudosLog, AwardedKudosEmployeeDto>();
        }
    }
}
