using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Committees;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Committee;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class CommitteeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommitteesService>().As<ICommitteesService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<CommitteeNotificationService>().As<ICommitteeNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
