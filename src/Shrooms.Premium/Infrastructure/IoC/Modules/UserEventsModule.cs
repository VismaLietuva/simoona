using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
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
