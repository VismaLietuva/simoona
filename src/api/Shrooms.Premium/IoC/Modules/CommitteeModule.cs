using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.Committees;
using Shrooms.Premium.Domain.Services.Email.Committee;

namespace Shrooms.Premium.IoC.Modules
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
