using Autofac;
using Hangfire;
using Shrooms.Premium.Infrastructure.Calendar;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class CalendarModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GoogleCalendarService>().As<ICalendarService>().InstancePerBackgroundJob();
        }
    }
}
