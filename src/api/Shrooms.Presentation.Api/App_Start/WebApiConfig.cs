using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Shrooms.Presentation.Api.GeneralCode;
using Shrooms.Presentation.Api.Handlers;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Presentation.Common.GeneralCode;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using WebApi.OutputCache.V2;

namespace Shrooms.Presentation.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableSystemDiagnosticsTracing();

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            config.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => new PerOrganizationCacheKeyGenerator());

            config.MessageHandlers.Add(new CancelledTaskBugWorkaroundMessageHandler());

            config.Services.Add(typeof(IExceptionLogger), new ApplicationInsightsExceptionsLogger());
            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new EmptyToNullConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new FormattedDecimalConverter());
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Ignoring reference loop handling is not good practice, because it generates JSON with additional (and big) overload (duplicated data).
            // Reference loops should be handled in one or another way and it's better to see when such errors are occuring to address it.
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}
