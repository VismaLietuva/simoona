using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Mvc;
using Autofac.Integration.WebApi;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using MVCControlsToolkit.Owin.Globalization;
using Owin;
using Shrooms.API.App_Start;
using Shrooms.API.Filters;
using Shrooms.API.GeneralCode;
using Shrooms.API.GeneralCode.SerializationIgnorer;
using Shrooms.API.ImageResizerPlugins;
using Shrooms.API.Middlewares;
using Shrooms.Constants.DataLayer;
using Shrooms.Constants.WebApi;
using Shrooms.Infrastructure.CloudScheduler;
using Shrooms.Infrastructure.CloudScheduler.Jobs;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email.Templating;
using Shrooms.IoC;

[assembly: OwinStartup(typeof(Shrooms.API.Startup))]

namespace Shrooms.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            ConfigureTelemetry();
            EmailTemplatesConfig.Register(AppDomain.CurrentDomain.BaseDirectory);
            SwaggerConfig.Setup(config);
            SerializationIgnoreConfigs.Configure();
            RouteConfig.Register(config);
            WebApiConfig.Register(config);
            FilterConfig.RegisterGlobalMvcFilters(GlobalFilters.Filters);
            FilterConfig.RegisterGlobalWebApiFilters(config.Filters);

            ConfigureAuthMiddleware(app);

            app.UseCors(SetupCorsOptions());
            app.Use<ImageResizerMiddleware>();
            app.Use<MultitenancyMiddleware>();

            var container = IocBootstrapper.Bootstrap(app, ExtractConnString, config);
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);

            StartBackgroundWorker(app);

            ConfigureAuthServer(app, container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            SetupGlobalization(app);
            ConfigureSignalr(app);
            app.UseWebApi(config);

            // InitiateScheduledJobs();
        }

        private void ConfigureSignalr(IAppBuilder app)
        {
            app.Map(
                "/signalr",
                map =>
                {
                    var hubConfig = new HubConfiguration { EnableDetailedErrors = true };
                    var authorizer = new QueryStringBearerAuthorizeAttribute();
                    var module = new AuthorizeModule(authorizer, authorizer);
                    GlobalHost.HubPipeline.AddModule(module);
                    map.RunSignalR(hubConfig);
                });
        }

        private static void StartBackgroundWorker(IAppBuilder app)
        {
            var interval = ConfigurationManager.AppSettings["BackgroundWorkerSqlPollingIntervalInSeconds"];
            var options = new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(Convert.ToInt16(interval)) };

            Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage(ConstDataLayer.ConnectionStringNameBackgroundJobs, options);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }

        private static string ExtractConnString()
        {
            var owinCtx = HttpContext.Current.GetOwinContext();
            var org = owinCtx.Get<string>("tenantName");
            return org;
        }

        private static CorsOptions SetupCorsOptions()
        {
            var corsPolicy = new CorsPolicy { AllowAnyMethod = true, AllowAnyHeader = true, SupportsCredentials = true, PreflightMaxAge = short.MaxValue };

            // Try and load allowed origins from web.config
            // If none are specified we'll allow all origins
            var appSetting = new ApplicationSettings();
            var origins = appSetting.CorsOriginsSetting;

            if (!string.IsNullOrEmpty(origins))
            {
                foreach (var origin in origins.Split(';'))
                {
                    corsPolicy.Origins.Add(origin);
                }
            }
            else
            {
                corsPolicy.AllowAnyOrigin = true;
            }

            var corsOptions = new CorsOptions { PolicyProvider = new CorsPolicyProvider { PolicyResolver = context => Task.FromResult(corsPolicy) } };
            return corsOptions;
        }

        private static void SetupGlobalization(IAppBuilder app)
        {
            var globalizationOptions = new OwinGlobalizationOptions(ConstWebApi.DefaultLanguage)
                .CustomCookieName(ConstWebApi.LanguageCookieName)
                .DisableCultureInPath();

            foreach (var language in ConstWebApi.SupportedLanguages)
            {
                globalizationOptions.Add(language, true);
            }

            app.UseGlobalization(globalizationOptions);
        }

        private static void InitiateScheduledJobs()
        {
            var isProductionEnvironment = bool.Parse(ConfigurationManager.AppSettings["IsProductionBuild"]);
            if (!isProductionEnvironment)
            {
                return;
            }

            var appSettings = new ApplicationSettings();
            var tenants = OrganizationUtils.AvailableOrganizations.Values.ToArray();

            foreach (var tenant in tenants)
            {
                BackgroundJob.Enqueue(() => AzureJobScheduler.Launch(new BirthdayNotifier(tenant, appSettings)));
                BackgroundJob.Enqueue(() => AzureJobScheduler.Launch(new RecurringEvents(tenant, appSettings)));
            }
        }

        private static void ConfigureTelemetry()
        {
            var isTelemetryEnabled = bool.Parse(ConfigurationManager.AppSettings["EnableAITelemetry"]);
            if (isTelemetryEnabled)
            {
                TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["AIInstrumentationKey"];
                ConfigureTelemetryFilter();
            }
            else
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }
        }

        private static void ConfigureTelemetryFilter()
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use(next => new UnwantedTelemetryFilter(next));
            builder.Build();
        }
    }
}