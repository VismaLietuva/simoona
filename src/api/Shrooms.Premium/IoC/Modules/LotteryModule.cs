using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.Lotteries;

namespace Shrooms.Premium.IoC.Modules
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

            builder.RegisterType<LotteryExportService>()
                 .As<ILotteryExportService>()
                 .InstancePerRequest()
                 .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryAbortJob>()
                .As<ILotteryAbortJob>()
                .EnableInterfaceTelemetryInterceptor();
        }
    }
}
