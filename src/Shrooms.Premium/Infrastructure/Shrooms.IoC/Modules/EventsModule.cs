using Autofac;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Domain.Services.Events.Export;
using Shrooms.Domain.Services.Events.List;
using Shrooms.Domain.Services.Events.Participation;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.DomainServiceValidators.Validators.Events;

namespace Shrooms.IoC.Modules
{
    public class EventsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventService>().As<IEventService>().InstancePerRequest();
            builder.RegisterType<EventNotificationService>().As<IEventNotificationService>();
            builder.RegisterType<EventExportService>().As<IEventExportService>().InstancePerRequest();
            builder.RegisterType<EventListingService>().As<IEventListingService>().InstancePerRequest();
            builder.RegisterType<EventCalendarService>().As<IEventCalendarService>().InstancePerRequest();
            builder.RegisterType<EventUtilitiesService>().As<IEventUtilitiesService>().InstancePerRequest();
            builder.RegisterType<EventValidationService>().As<IEventValidationService>().InstancePerRequest();
            builder.RegisterType<EventParticipationService>().As<IEventParticipationService>().InstancePerRequest();
        }
    }
}
