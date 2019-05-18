using Autofac;
using Shrooms.Premium.Infrastructure.Shrooms.Infrastructure.VacationBot;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Vacations;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
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