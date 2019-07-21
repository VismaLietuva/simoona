using Autofac;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.UserService;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Infrastructure.GoogleBookApiService;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<GoogleBookService>().As<IBookInfoService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}