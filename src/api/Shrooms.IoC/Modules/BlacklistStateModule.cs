using Autofac;
using Shrooms.Domain.Services.BlacklistStates;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class BlacklistStateModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlacklistStateService>()
                .As<IBlacklistStateService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<BlacklistStateValidator>()
                .As<IBlacklistStateValidator>()
                .InstancePerRequest();
        }
    }
}
