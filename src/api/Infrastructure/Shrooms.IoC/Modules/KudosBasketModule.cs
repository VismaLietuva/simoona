using Autofac;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.DomainServiceValidators.Validators.KudosBaskets;

namespace Shrooms.IoC.Modules
{
    public class KudosBasketModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosBasketValidator>().As<IKudosBasketValidator>().InstancePerRequest();
            builder.RegisterType<KudosBasketService>().As<IKudosBasketService>().InstancePerRequest();
        }
    }
}
