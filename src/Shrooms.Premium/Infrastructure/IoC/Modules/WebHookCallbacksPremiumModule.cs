using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
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
