using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class AsyncRunner : IAsyncRunner
    {
        public ILifetimeScope LifetimeScope { get; set; }

        public AsyncRunner(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
        }

        public void Run<T>(Func<T, Task> action, string tenantName)
        {
            // ASP.NET Core equivalent: use Task.Run for background work
            // In production, consider using IHostedService or BackgroundService
            Task.Run(async () =>
            {
                using (var container = LifetimeScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag,
                    builder => { builder.RegisterInstance(new TenantNameContainer(tenantName)).As<ITenantNameContainer>().SingleInstance(); }))
                {
                    var logger = container.Resolve<ILogger>();
                    var service = container.Resolve<T>();
                    try
                    {
                        await action(service);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            });
        }
    }
}
