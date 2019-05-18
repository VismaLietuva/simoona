using Autofac;
using Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.DomainServiceValidators.Validators.Kudos;

namespace Shrooms.IoC.Modules
{
    public class KudosModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosServiceValidator>().As<IKudosServiceValidator>().InstancePerRequest();
            builder.RegisterType<KudosService>().As<IKudosService>().InstancePerRequest();
            builder.RegisterType<KudosExportService>().As<IKudosExportService>().InstancePerRequest();
            builder.RegisterType<KudosNotificationService>().As<IKudosNotificationService>().InstancePerRequest();
        }
    }
}