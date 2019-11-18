using AutoMapper;
using Shrooms.ModelMappings.Profiles;
using System;

namespace Shrooms.Premium.UnitTests.ModelMappings
{
    public class ModelMapper
    {
        public static IMapper Create()
        {
            Action<IMapperConfiguration> mappings = cfg =>
            {
                cfg.AddProfile<OrganizationalStructureProfile>();
                cfg.AddProfile<ServiceRequestProfile>();
                cfg.AddProfile<CommitteeProfile>();
                cfg.AddProfile<KudosShopProfile>();
                cfg.AddProfile<LoyaltyKudosProfile>();
                cfg.AddProfile<EventsProfile>();
                cfg.AddProfile<VacationsProfile>();
                cfg.AddProfile<LotteryProfile>();
            };

            var configuration = new MapperConfiguration(mappings);
            return configuration.CreateMapper();
        }
    }
}
