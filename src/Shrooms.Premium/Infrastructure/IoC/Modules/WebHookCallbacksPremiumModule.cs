using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class WebHookCallbacksPremiumModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosPremiumNotificationService>().As<IKudosPremiumNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<LoyaltyKudosService>().As<ILoyaltyKudosService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<LoyaltyKudosCalculator>().As<ILoyaltyKudosCalculator>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<BookRemindService>().As<IBookRemindService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<EventsWebHookService>().As<IEventsWebHookService>().InstancePerRequest(); //.EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<WebHookCallbackPremiumServices>().As<IWebHookCallbackPremiumServices>().InstancePerRequest().PropertiesAutowired(); //.EnableInterfaceTelemetryInterceptor();
        }
    }
}