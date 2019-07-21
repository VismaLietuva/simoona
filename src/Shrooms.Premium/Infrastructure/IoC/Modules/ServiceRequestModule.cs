using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.ServiceRequest;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.ServiceRequests;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
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
