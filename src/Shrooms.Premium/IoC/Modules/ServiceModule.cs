using Autofac;
using Shrooms.Infrastructure.GoogleBookApiService;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.Notifications;

namespace Shrooms.Premium.IoC.Modules
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
