using Autofac;
using Shrooms.Domain.Services.Employees;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class EmployeeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmployeeListingService>()
                .As<IEmployeeListingService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();
        }
    }
}
