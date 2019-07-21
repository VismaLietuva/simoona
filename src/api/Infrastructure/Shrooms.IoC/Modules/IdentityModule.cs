using Autofac;
using Microsoft.AspNet.Identity;
using Shrooms.Authentification;
using Shrooms.Authorization.BasicAuth;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Interceptors;
using Module = Autofac.Module;

namespace Shrooms.IoC.Modules
{
    public class IdentityModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MailingService>().As<IIdentityMessageService>().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ShroomsRoleStore>().As<IRoleStore<ApplicationRole, string>>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ShroomsRoleManager>().InstancePerRequest();
            builder.RegisterType<RoleManager<ApplicationRole, string>>().AsSelf().InstancePerRequest().EnableClassTelemetryInterceptor();
            builder.RegisterType<ShroomsUserStore>().As<IUserStore<ApplicationUser>>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ShroomsUserManager>().AsSelf().InstancePerRequest().EnableClassTelemetryInterceptor();
            builder.RegisterType<ShroomsClaimsIdentityFactory>().AsSelf().InstancePerRequest();
            builder.RegisterType<BasicAuthValidator>().As<IBasicAuthValidator>();
        }
    }
}