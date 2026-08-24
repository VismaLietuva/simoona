using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Seats;

namespace Shrooms.IoC.Modules
{
    public static class SeatModule
    {
        public static IServiceCollection AddSeats(this IServiceCollection services)
        {
            services.AddScoped<ISeatService, SeatService>();
            return services;
        }
    }
}
