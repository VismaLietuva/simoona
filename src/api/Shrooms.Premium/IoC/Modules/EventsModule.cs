using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Premium.Domain.Services.Events.Calendar;
using Shrooms.Premium.Domain.Services.Events.Export;
using Shrooms.Premium.Domain.Services.Events.List;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;

namespace Shrooms.Premium.IoC.Modules
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
