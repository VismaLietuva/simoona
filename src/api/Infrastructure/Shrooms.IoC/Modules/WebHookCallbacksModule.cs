using Autofac;
using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;

namespace Shrooms.IoC.Modules
{
    public class WebHookCallbacksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BirthdaysNotificationWebHookService>().As<IBirthdaysNotificationWebHookService>().InstancePerRequest();

            builder.RegisterType<WebHookCallbackServices>().As<IWebHookCallbackServices>().InstancePerRequest().PropertiesAutowired();
        }
    }
}