using System;
using AutoMapper;
using Shrooms.ModelMappings.Profiles;

namespace Shrooms.UnitTests.ModelMappings
{
    public class ModelMapper
    {
        public static IMapper Create()
        {
            Action<IMapperConfiguration> mappings = cfg =>
            {
                cfg.AddProfile<Posts>();
                cfg.AddProfile<KudosBasket>();
                cfg.AddProfile<Kudos>();
                cfg.AddProfile<Birthdays>();
                cfg.AddProfile<Books>();
                cfg.AddProfile<Comments>();
                cfg.AddProfile<Users>();
                cfg.AddProfile<Walls>();
                cfg.AddProfile<Other>();
                cfg.AddProfile<ExternalLinks>();
                cfg.AddProfile<Monitors>();
                cfg.AddProfile<Roles>();
                cfg.AddProfile<Permissions>();
                cfg.AddProfile<Projects>();
                cfg.AddProfile<Jobs>();
                cfg.AddProfile<Notifications>();
            };

            var configuration = new MapperConfiguration(mappings);
            return configuration.CreateMapper();
        }
    }
}