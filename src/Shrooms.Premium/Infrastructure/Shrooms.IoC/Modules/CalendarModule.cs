using Autofac;
using Hangfire;
using Shrooms.Infrastructure.Calendar;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class CalendarModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GoogleCalendarService>().As<ICalendarService>().InstancePerBackgroundJob().EnableInterfaceTelemetryInterceptor();
        }
    }
}
