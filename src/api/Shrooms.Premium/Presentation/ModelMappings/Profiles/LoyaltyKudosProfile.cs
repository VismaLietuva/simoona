using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class LoyaltyKudosProfile : Profile
    {
        public LoyaltyKudosProfile()
        {
            CreateLoyaltyKudosMappings();
        }

        private void CreateLoyaltyKudosMappings()
        {
            CreateMap<AwardedKudosEmployeeDto, KudosLog>(MemberList.None);
            CreateMap<KudosLog, AwardedKudosEmployeeDto>(MemberList.None);
        }
    }
}
