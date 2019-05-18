using Autofac;
using Hangfire;
using Shrooms.Premium.Infrastructure.Shrooms.Infrastructure.Calendar;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class CalendarModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GoogleCalendarService>().As<ICalendarService>().InstancePerBackgroundJob();
        }
    }
}
