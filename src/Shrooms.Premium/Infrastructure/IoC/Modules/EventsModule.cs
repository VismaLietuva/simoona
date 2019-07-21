using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Calendar;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Export;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.List;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Participation;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Utilities;
using Shrooms.Premium.Main.BusinessLayer.DomainServiceValidators.Events;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class EventsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventService>().As<IEventService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventNotificationService>().As<IEventNotificationService>().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventExportService>().As<IEventExportService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventListingService>().As<IEventListingService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventCalendarService>().As<IEventCalendarService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventUtilitiesService>().As<IEventUtilitiesService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventValidationService>().As<IEventValidationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<EventParticipationService>().As<IEventParticipationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
