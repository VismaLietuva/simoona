using Autofac;
using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class WebHookCallbacksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BirthdaysNotificationWebHookService>().As<IBirthdaysNotificationWebHookService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<WebHookCallbackServices>().As<IWebHookCallbackServices>().InstancePerRequest().PropertiesAutowired().EnableInterfaceTelemetryInterceptor();
        }
    }
}