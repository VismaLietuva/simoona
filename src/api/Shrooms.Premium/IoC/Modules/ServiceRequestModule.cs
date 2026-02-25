using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Email.ServiceRequest;
using Shrooms.Premium.Domain.Services.ServiceRequests;

namespace Shrooms.Premium.IoC.Modules
{
    public static class ServiceRequestModule
    {
        public static IServiceCollection AddPremiumServiceRequest(this IServiceCollection services)
        {
            services.AddScoped<IServiceRequestService, ServiceRequestService>();
            services.AddScoped<IServiceRequestNotificationService, ServiceRequestNotificationService>();
            services.AddScoped<IServiceRequestExportService, ServiceRequestExportService>();
            return services;
        }
    }
}