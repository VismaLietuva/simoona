using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Employees;

namespace Shrooms.IoC.Modules
{
    public static class EmployeeModule
    {
        public static IServiceCollection AddEmployees(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeListingService, EmployeeListingService>();
            return services;
        }
    }
}
