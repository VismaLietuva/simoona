using Autofac;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class WebHookCallbacksPremiumModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosPremiumNotificationService>().As<IKudosPremiumNotificationService>().InstancePerRequest();
            builder.RegisterType<LoyaltyKudosService>().As<ILoyaltyKudosService>().InstancePerRequest();
            builder.RegisterType<EventsWebHookService>().As<IEventsWebHookService>().InstancePerRequest();

            builder.RegisterType<WebHookCallbackPremiumServices>().As<IWebHookCallbackPremiumServices>().InstancePerRequest().PropertiesAutowired();
        }
    }
}
