using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;

namespace Shrooms.IoC.Modules
{
    public static class FilterPresetModule
    {
        public static IServiceCollection AddFilterPresets(this IServiceCollection services)
        {
            services.AddScoped<IFilterPresetService, FilterPresetService>();
            services.AddScoped<IFilterPresetValidator, FilterPresetValidator>();
            return services;
        }
    }
}
