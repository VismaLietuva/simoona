using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Domain.Services.Lotteries;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class LotteryParticipantModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ParticipantService>().As<IParticipantService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}