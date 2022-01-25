using Autofac;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.VacationPages;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<VacationPageService>().As<IVacationPageService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}