using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Committees;
using Shrooms.Premium.Domain.Services.Email.Committee;

namespace Shrooms.Premium.IoC.Modules
{
    public static class CommitteeModule
    {
        public static IServiceCollection AddPremiumCommittee(this IServiceCollection services)
        {
            services.AddScoped<ICommitteesService, CommitteesService>();
            services.AddScoped<ICommitteeNotificationService, CommitteeNotificationService>();
            return services;
        }
    }
}