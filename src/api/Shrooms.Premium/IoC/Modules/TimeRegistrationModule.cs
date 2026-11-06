using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Email.TimeRegistration;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.IoC.Modules
{
    public static class TimeRegistrationModule
    {
        public static IServiceCollection AddPremiumTimeRegistration(this IServiceCollection services)
        {
            services.AddScoped<ITimeRegistrationSupport, TimeRegistrationSupport>();
            services.AddScoped<ITimeAccountingService, TimeAccountingService>();
            services.AddScoped<ITimeTableService, TimeTableService>();
            services.AddScoped<ITimePeriodService, TimePeriodService>();
            services.AddScoped<ITimeReportService, TimeReportService>();
            services.AddScoped<ITimeOvertimeService, TimeOvertimeService>();
            services.AddScoped<ITimeOvertimeNotificationService, TimeOvertimeNotificationService>();
            return services;
        }
    }
}
