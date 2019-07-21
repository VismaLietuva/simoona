using Autofac;
using Shrooms.Domain.Services.Email.ServiceRequest;
using Shrooms.Domain.Services.ServiceRequests;
using Shrooms.Domain.Services.ServiceRequests.Export;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
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
