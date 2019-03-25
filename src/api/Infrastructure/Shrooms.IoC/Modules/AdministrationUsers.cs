using Autofac;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Email.AdministrationUsers;

namespace Shrooms.IoC.Modules
{
    public class AdministrationUsers : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AdministrationUsersService>().As<IAdministrationUsersService>().InstancePerRequest();
            builder.RegisterType<AdministrationUsersNotificationService>().As<IAdministrationNotificationService>().InstancePerRequest();
        }
    }
}
