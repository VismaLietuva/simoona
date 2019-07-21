using Autofac;
using Shrooms.Domain.Services.Vacations;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Infrastructure.VacationBot;

namespace Shrooms.IoC.Modules
{
    public class VacationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VacationHistoryService>().As<IVacationHistoryService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<VacationService>().As<IVacationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<VacationBotService>().As<IVacationBotService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<VacationDomainService>().As<IVacationDomainService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}