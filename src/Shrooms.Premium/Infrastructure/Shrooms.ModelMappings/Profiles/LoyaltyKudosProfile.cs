using AutoMapper;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;

namespace Shrooms.ModelMappings.Profiles
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
