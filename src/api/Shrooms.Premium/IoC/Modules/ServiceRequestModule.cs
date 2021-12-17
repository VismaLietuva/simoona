using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.Email.ServiceRequest;
using Shrooms.Premium.Domain.Services.ServiceRequests;

namespace Shrooms.Premium.IoC.Modules
{
    public class ServiceRequestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceRequestService>().As<IServiceRequestService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ServiceRequestNotificationService>().As<IServiceRequestNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ServiceRequestExportService>().As<IServiceRequestExportService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
