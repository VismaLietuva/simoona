using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DomainServiceValidators.Validators.KudosBaskets;
using Shrooms.Domain.Services.KudosBaskets;

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
