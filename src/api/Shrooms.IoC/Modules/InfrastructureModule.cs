using System.Configuration;
using Autofac;
using Autofac.Core.Lifetime;
using Hangfire;
using ReallySimpleFeatureToggle;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.CustomCache;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Infrastructure.Logger;
using Shrooms.Infrastructure.Storage;
using Shrooms.Infrastructure.Storage.AzureBlob;
using Shrooms.Infrastructure.Storage.FileSystem;
using Shrooms.Infrastructure.SystemClock;

namespace Shrooms.IoC.Modules
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Logger>().As<ILogger>().InstancePerMatchingLifetimeScope(AutofacJobActivator.LifetimeScopeTag, MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            builder.RegisterType<CommonMarkMarkdownConverter>().As<IMarkdownConverter>().InstancePerRequest();
            builder.RegisterType<MailingService>().As<IMailingService>()
                .InstancePerMatchingLifetimeScope(AutofacJobActivator.LifetimeScopeTag, MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterGeneric(typeof(CustomCache<,>)).As(typeof(ICustomCache<,>)).SingleInstance();
            builder.RegisterType<ApplicationSettings>().As<IApplicationSettings>().InstancePerRequest();
            builder.RegisterType<ApplicationSettings>().As<IApplicationSettings>().InstancePerBackgroundJob();
            builder.RegisterType<ApplicationSettings>().As<IApplicationSettings>().InstancePerLifetimeScope();

            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();
            builder.RegisterType<ExcelBuilder>().As<IExcelBuilder>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<MailTemplate>().As<IMailTemplate>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<DailyMailingService>().As<IDailyMailingService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<HangFireScheduler>().As<IJobScheduler>().InstancePerRequest();
            builder.Register(c => ReallySimpleFeature.Toggles.GetFeatureConfiguration()).As<IFeatureConfiguration>().SingleInstance();

            RegisterStorage(builder);
        }

        private void RegisterStorage(ContainerBuilder builder)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString))
            {
                builder.RegisterType<FileSystemStorage>().As<IStorage>().InstancePerRequest();
            }
            else
            {
                builder.RegisterType<AzureStorage>().As<IStorage>().InstancePerRequest();
            }
        }
    }
}