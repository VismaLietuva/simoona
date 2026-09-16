using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Email.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Infrastructure.VacationBot;

namespace Shrooms.Premium.IoC.Modules
{
    public static class VacationModule
    {
        public static IServiceCollection AddPremiumVacation(this IServiceCollection services)
        {
            services.AddScoped<IVacationHistoryService, VacationHistoryService>();
            services.AddScoped<IVacationService, VacationService>();
            services.AddHttpClient<IVacationBotService, VacationBotService>();
            services.AddScoped<IVacationDomainService, VacationDomainService>();

            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IVacationRequestService, VacationRequestService>();
            services.AddScoped<IVacationRequestListService, VacationRequestListService>();
            services.AddScoped<IVacationLogService, VacationLogService>();
            services.AddScoped<IVacationStatisticsService, VacationStatisticsService>();
            services.AddScoped<IVacationSettingsService, VacationSettingsService>();
            services.AddScoped<IVacationReportService, VacationReportService>();
            services.AddScoped<IVacationOrderService, VacationOrderService>();
            services.AddScoped<IVacationNotificationService, VacationNotificationService>();

            return services;
        }
    }
}