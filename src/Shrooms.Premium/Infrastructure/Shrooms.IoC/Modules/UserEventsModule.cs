using Autofac;
using Shrooms.Domain.Services.Users;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
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
