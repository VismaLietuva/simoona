using Autofac;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Email.AdministrationUsers;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class AdministrationUsers : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AdministrationUsersService>().As<IAdministrationUsersService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<AdministrationUsersNotificationService>().As<IAdministrationNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
