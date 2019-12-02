using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;
using Shrooms.Infrastructure.GoogleBookService;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<GoogleBookService>().As<IBookInfoService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
