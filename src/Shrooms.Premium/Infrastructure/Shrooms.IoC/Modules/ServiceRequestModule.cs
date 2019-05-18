using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.ServiceRequest;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.ServiceRequests;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
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
