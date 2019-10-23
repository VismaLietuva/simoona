using Autofac;
using Shrooms.Domain.Services.Committees;
using Shrooms.Domain.Services.Email.Committee;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class LotteryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LotteryService>()
                .As<ILotteryService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<ParticipantService>()
                .As<IParticipantService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryAbortJob>()
                .As<ILotteryAbortJob>()
                .EnableInterfaceTelemetryInterceptor();
        }
    }
}
