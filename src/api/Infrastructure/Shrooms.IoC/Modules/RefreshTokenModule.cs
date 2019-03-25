using Autofac;
using Shrooms.Domain.Services.RefreshTokens;

namespace Shrooms.IoC.Modules
{
    public class RefreshTokenModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RefreshTokenService>().As<IRefreshTokenService>().InstancePerRequest();
        }
    }
}
