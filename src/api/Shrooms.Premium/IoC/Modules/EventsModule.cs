using Microsoft.Extensions.DependencyInjection;
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
    public static class EventsModule
    {
        public static IServiceCollection AddPremiumEvents(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventNotificationService, EventNotificationService>();
            services.AddScoped<IEventExportService, EventExportService>();
            services.AddScoped<IEventListingService, EventListingService>();
            services.AddScoped<IEventCalendarService, EventCalendarService>();
            services.AddScoped<IEventUtilitiesService, EventUtilitiesService>();
            services.AddScoped<IEventValidationService, EventValidationService>();
            services.AddScoped<IEventParticipationService, EventParticipationService>();
            return services;
        }
    }
}