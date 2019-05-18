using AutoMapper;
using System;
using Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles;

namespace Shrooms.Premium.UnitTests.ModelMappings
{
    public static class ModelMapper
    {
        public static IMapper Create()
        {
            Action<IMapperConfiguration> mappings = cfg =>
            {
                cfg.AddProfile<OrganizationalStructureProfile>();
                cfg.AddProfile<ServiceRequestProfile>();
                cfg.AddProfile<CommitteeProfile>();
                cfg.AddProfile<KudosShopProfile>();
                cfg.AddProfile<EventsProfile>();
                cfg.AddProfile<VacationsProfile>();
            };

            var configuration = new MapperConfiguration(mappings);
            return configuration.CreateMapper();
        }
    }
}
