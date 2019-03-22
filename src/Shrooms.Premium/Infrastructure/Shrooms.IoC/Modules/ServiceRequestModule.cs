using Autofac;
using Shrooms.Domain.Services.Email.ServiceRequest;
using Shrooms.Domain.Services.ServiceRequests;
using Shrooms.Domain.Services.ServiceRequests.Export;

namespace Shrooms.IoC.Modules
{
    public class ServiceRequestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceRequestService>().As<IServiceRequestService>().InstancePerRequest();
            builder.RegisterType<ServiceRequestNotificationService>().As<IServiceRequestNotificationService>().InstancePerRequest();
            builder.RegisterType<ServiceRequestExportService>().As<IServiceRequestExportService>().InstancePerRequest();
        }
    }
}
