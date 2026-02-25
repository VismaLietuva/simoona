using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Jobs;

namespace Shrooms.IoC.Modules
{
    public static class JobModule
    {
        public static IServiceCollection AddJobs(this IServiceCollection services)
        {
            services.AddScoped<IJobService, JobService>();
            return services;
        }
    }
}
