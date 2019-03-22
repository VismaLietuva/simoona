using Autofac;
using Shrooms.Domain.Services.Vacations;
using Shrooms.Infrastructure.VacationBot;

namespace Shrooms.IoC.Modules
{
    public class VacationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VacationHistoryService>().As<IVacationHistoryService>().InstancePerRequest();
            builder.RegisterType<VacationService>().As<IVacationService>().InstancePerRequest();
            builder.RegisterType<VacationBotService>().As<IVacationBotService>().InstancePerRequest();
            builder.RegisterType<VacationDomainService>().As<IVacationDomainService>().InstancePerRequest();
        }
    }
}