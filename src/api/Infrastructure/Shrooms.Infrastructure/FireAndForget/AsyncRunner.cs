using System;
using System.Web.Hosting;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Features.OwnedInstances;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class AsyncRunner : IAsyncRunner
    {
        public ILifetimeScope LifetimeScope { get; set; }

        public AsyncRunner(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
        }

        public void Run<T>(Action<T> action, string tenantName)
        {
            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                using (var container = LifetimeScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, (builder) =>
                 {
                     builder.RegisterInstance(new TenantNameContainer(tenantName)).As<ITenantNameContainer>().SingleInstance();
                 }))
                {
                    var service = container.Resolve<T>();
                    action(service);
                }
            });
        }
    }

    public interface IAsyncRunner
    {
        void Run<T>(Action<T> action, string tenantName);
    }
}
