using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using NLog;
using Shrooms.API.GeneralCode;
using Shrooms.API.Handlers;
using Shrooms.API.Helpers;
using Shrooms.Infrastructure.Configuration;
using WebApi.OutputCache.V2;

namespace Shrooms.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableSystemDiagnosticsTracing();
            var applicationSettings = new ApplicationSettings();

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //config.IncludeErrorDetailPolicy = applicationSettings.IsProductionBuild ? IncludeErrorDetailPolicy.Never : IncludeErrorDetailPolicy.Always;

            config.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => new PerOrganizationCacheKeyGenerator());

            config.MessageHandlers.Add(new CancelledTaskBugWorkaroundMessageHandler());

            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());

            //config.Services.Replace(typeof(IHttpControllerSelector), new HttpNotFoundAwareDefaultHttpControllerSelector(config));
            //config.Services.Replace(typeof(IHttpActionSelector), new HttpNotFoundAwareControllerActionSelector());

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new EmptyToNullConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new FormattedDecimalConverter());
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Ignoring reference loop handling is not good practice, because it generates JSON with additional (and big) overload (duplicated data).
            // Reference loops should be handled in one or another way and it's better to see when such errors are occuring to address it.
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }

        private class NLogExceptionLogger : ExceptionLogger
        {
            private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

            public override void Log(ExceptionLoggerContext context)
            {
                Nlog.Log(LogLevel.Error, context.Exception);
            }
        }
    }
}