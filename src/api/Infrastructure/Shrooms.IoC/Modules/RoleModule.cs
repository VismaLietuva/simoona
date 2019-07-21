using Autofac;
using Shrooms.Domain.Services.Roles;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class RoleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RoleService>().As<IRoleService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
