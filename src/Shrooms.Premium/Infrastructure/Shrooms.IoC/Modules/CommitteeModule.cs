using Autofac;
using Shrooms.Domain.Services.Committees;
using Shrooms.Domain.Services.Email.Committee;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
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
