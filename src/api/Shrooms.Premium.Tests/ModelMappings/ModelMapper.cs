using AutoMapper;
using Shrooms.Premium.Presentation.ModelMappings.Profiles;

namespace Shrooms.Premium.Tests.ModelMappings
{
    public static class ModelMapper
    {
        public static IMapper Create()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrganizationalStructureProfile>();
                cfg.AddProfile<ServiceRequestProfile>();
                cfg.AddProfile<CommitteeProfile>();
                cfg.AddProfile<GroupProfile>();
                cfg.AddProfile<KudosShopProfile>();
                cfg.AddProfile<LoyaltyKudosProfile>();
                cfg.AddProfile<EventsProfile>();
                cfg.AddProfile<VacationsProfile>();
                cfg.AddProfile<LotteryProfile>();
            });
            return configuration.CreateMapper();
        }
    }
}
