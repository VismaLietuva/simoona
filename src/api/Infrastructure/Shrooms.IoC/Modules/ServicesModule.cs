using Autofac;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.UserService;
using Shrooms.Infrastructure.GoogleBookApiService;

namespace Shrooms.IoC.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerRequest();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest();
            builder.RegisterType<GoogleBookService>().As<IBookInfoService>().InstancePerRequest();
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerRequest();
        }
    }
}