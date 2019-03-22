using Autofac;
using Shrooms.Domain.Services.Committees;
using Shrooms.Domain.Services.Email.Committee;

namespace Shrooms.IoC.Modules
{
    public class CommitteeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommitteesService>().As<ICommitteesService>().InstancePerRequest();
            builder.RegisterType<CommitteeNotificationService>().As<ICommitteeNotificationService>().InstancePerRequest();
        }
    }
}
