using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.ServiceValidators.Validators.Kudos;

namespace Shrooms.IoC.Modules
{
    public static class KudosModule
    {
        public static IServiceCollection AddKudos(this IServiceCollection services)
        {
            services.AddScoped<IKudosServiceValidator, KudosServiceValidator>();
            services.AddScoped<IKudosService, KudosService>();
            services.AddScoped<IKudosExportService, KudosExportService>();
            services.AddScoped<IKudosNotificationService, KudosNotificationService>();
            return services;
        }
    }
}