using Autofac;
using Microsoft.AspNet.Identity;
using Shrooms.Authentification;
using Shrooms.Authorization.BasicAuth;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Email;
using Module = Autofac.Module;

namespace Shrooms.IoC.Modules
{
    public class IdentityModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MailingService>().As<IIdentityMessageService>();
            builder.RegisterType<ShroomsRoleStore>().As<IRoleStore<ApplicationRole, string>>().InstancePerRequest();
            builder.RegisterType<ShroomsRoleManager>().InstancePerRequest();
            builder.RegisterType<RoleManager<ApplicationRole, string>>().AsSelf().InstancePerRequest();
            builder.RegisterType<ShroomsUserStore>().As<IUserStore<ApplicationUser>>().InstancePerRequest();
            builder.RegisterType<ShroomsUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ShroomsClaimsIdentityFactory>().AsSelf().InstancePerRequest();
            builder.RegisterType<BasicAuthValidator>().As<IBasicAuthValidator>();
        }
    }
}