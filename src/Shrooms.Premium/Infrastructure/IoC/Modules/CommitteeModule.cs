using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Committees;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Committee;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
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
