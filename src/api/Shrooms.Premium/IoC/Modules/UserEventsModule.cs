using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.Users;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;

namespace Shrooms.Premium.IoC.Modules
{
    public class UserEventsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventRemindService>().As<IEventRemindService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<UserEventsService>().As<IUserEventsService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
