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
            builder.RegisterType<EventJoinRemindService>().As<IEventJoinRemindService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<UserEventsService>().As<IUserEventsService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
