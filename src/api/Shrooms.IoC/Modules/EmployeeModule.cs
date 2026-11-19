using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Employees;
using Shrooms.Domain.Services.Search;

namespace Shrooms.IoC.Modules
{
    public static class EmployeeModule
    {
        public static IServiceCollection AddEmployees(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeListingService, EmployeeListingService>();
            services.AddScoped<IGlobalSearchService, GlobalSearchService>();
            return services;
        }
    }
}
