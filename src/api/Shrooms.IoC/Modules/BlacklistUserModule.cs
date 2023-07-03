using Autofac;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;

namespace Shrooms.IoC.Modules
{
    public class BlacklistUserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlacklistService>()
                .As<IBlacklistService>()
                .InstancePerRequest();

            builder.RegisterType<BlacklistValidator>()
                .As<IBlacklistValidator>()
                .InstancePerRequest();
        }
    }
}
