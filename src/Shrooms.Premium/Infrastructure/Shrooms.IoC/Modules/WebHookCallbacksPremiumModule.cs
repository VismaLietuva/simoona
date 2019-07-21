using Autofac;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class WebHookCallbacksPremiumModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosPremiumNotificationService>().As<IKudosPremiumNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<LoyaltyKudosService>().As<ILoyaltyKudosService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventsWebHookService>().As<IEventsWebHookService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<WebHookCallbackPremiumServices>().As<IWebHookCallbackPremiumServices>().InstancePerRequest().PropertiesAutowired().EnableInterfaceTelemetryInterceptor();
        }
    }
}
