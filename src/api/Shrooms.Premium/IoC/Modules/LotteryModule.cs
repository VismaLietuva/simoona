using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.DomainServiceValidators.Lotteries;
using Shrooms.Premium.Domain.Services.Email.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;

namespace Shrooms.Premium.IoC.Modules
{
    public static class LotteryModule
    {
        public static IServiceCollection AddPremiumLottery(this IServiceCollection services)
        {
            services.AddScoped<ILotteryService, LotteryService>();
            services.AddScoped<ILotteryParticipantService, LotteryParticipantService>();
            services.AddScoped<ILotteryExportService, LotteryExportService>();
            services.AddScoped<ILotteryAbortJob, LotteryAbortJob>();
            services.AddScoped<ILotteryNotificationService, LotteryNotificationService>();
            services.AddScoped<ILotteryValidator, LotteryValidator>();
            return services;
        }
    }
}