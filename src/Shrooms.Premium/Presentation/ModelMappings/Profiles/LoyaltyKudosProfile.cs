using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class LoyaltyKudosProfile : Profile
    {
        protected override void Configure()
        {
            CreateLoyaltyKudosMappings();
        }

        private void CreateLoyaltyKudosMappings()
        {
            CreateMap<AwardedKudosEmployeeDTO, KudosLog>();
            CreateMap<KudosLog, AwardedKudosEmployeeDTO>();
        }
    }
}
