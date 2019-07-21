using Autofac;
using DomainServiceValidators.Validators.KudosBaskets;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class KudosBasketModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosBasketValidator>().As<IKudosBasketValidator>().InstancePerRequest();
            builder.RegisterType<KudosBasketService>().As<IKudosBasketService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
