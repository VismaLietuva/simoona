using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Polls;

namespace Shrooms.IoC.Modules
{
    public static class PollModule
    {
        public static IServiceCollection AddPolls(this IServiceCollection services)
        {
            services.AddScoped<IPollService, PollService>();
            return services;
        }
    }
}
