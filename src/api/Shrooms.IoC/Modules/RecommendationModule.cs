using Autofac;
using Shrooms.Domain.Services.Recommendation;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules 
{
    internal class RecommendationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RecommendationService>().As<IRecommendationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
