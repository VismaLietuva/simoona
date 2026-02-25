using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class AsyncRunner : IAsyncRunner
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AsyncRunner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Run<T>(Func<T, Task> action, string tenantName)
        {
            Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();

                var tenantContainer = scope.ServiceProvider.GetService<ITenantNameContainer>();
                if (tenantContainer != null)
                {
                    tenantContainer.TenantName = tenantName;
                }

                var logger = scope.ServiceProvider.GetService<ILogger>();
                var service = scope.ServiceProvider.GetRequiredService<T>();
                try
                {
                    await action(service);
                }
                catch (Exception ex)
                {
                    logger?.Error(ex);
                }
            });
        }
    }
}
