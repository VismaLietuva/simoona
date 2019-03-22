using Autofac;
using Hangfire;
using Shrooms.Infrastructure.Calendar;

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
