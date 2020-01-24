using Autofac;
using Shrooms.Infrastructure.GoogleBookApiService;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
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
