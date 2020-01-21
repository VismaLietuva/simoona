using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Shrooms.DataTransferObjects.Models.CalendarEvent;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Logger;
using Shrooms.Infrastructure.Retryer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Shrooms.Infrastructure.Calendar
{
    public class GoogleCalendarService : ICalendarService
    {
        private const string EventIdStringFormat = "N";

        private readonly IApplicationSettings _appSettings;
        private readonly ILogger _logger;

        public GoogleCalendarService()
        {
            _appSettings = new ApplicationSettings();
            _logger = new Logger.Logger();
        }

        public void DeleteEvent(Guid eventId, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            var formattedEventId = ConvertGuidToFormattedString(eventId);
            var request = calendar.Events.Delete(tenantCalendarId, formattedEventId);
            request.SendNotifications = true;

            try
            {
                request.Execute();
            }
            catch (GoogleApiException e)
            {
                _logger.Error(e);
            }
        }

        public void UpdateEvent(CalendarEventDTO calendarEventDto, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            Action updateEvent = () =>
            {
                var @event = GetEvent(calendarEventDto.EventId, tenantCalendarId);
                if (@event == null)
                {
                    return;
                }

                @event.Summary = calendarEventDto.Name;
                @event.Description = calendarEventDto.Description;
                @event.Start = ParseToEventDateTime(calendarEventDto.StartDate);
                @event.End = ParseToEventDateTime(calendarEventDto.EndDate);
                @event.Location = calendarEventDto.Location;

                var request = calendar.Events.Update(@event, tenantCalendarId, @event.Id);
                request.SendNotifications = true;

                request.Execute();
            };

            try
            {
                Retry.Do(updateEvent, TimeSpan.FromMilliseconds(500), 5);
            }
            catch (AggregateException e)
            {
                _logger.Error(e);
            }
        }

        public void CreateEvent(CalendarEventDTO calendarEvent, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            var @event = new Event()
            {
                Id = ConvertGuidToFormattedString(calendarEvent.EventId),
                Summary = calendarEvent.Name,
                Description = calendarEvent.Description,
                Start = ParseToEventDateTime(calendarEvent.StartDate),
                End = ParseToEventDateTime(calendarEvent.EndDate),
                Visibility = "private",
                GuestsCanInviteOthers = false,
                Location = calendarEvent.Location,
                GuestsCanModify = false,
            };

            var request = calendar.Events.Insert(@event, tenantCalendarId);

            try
            {
                request.Execute();
            }
            catch (GoogleApiException e)
            {
                _logger.Error(e);
            }
        }

        public void AddParticipants(Guid eventId, string tenantCalendarId, IEnumerable<string> emails, IEnumerable<string> eventChoices)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            Action addParticipants = () =>
            {
                var @event = GetEvent(eventId, tenantCalendarId);
                if (@event == null)
                {
                    return;
                }

                if (@event.Attendees == null)
                {
                    @event.Attendees = new List<EventAttendee>();
                }

                foreach (var email in emails)
                {
                    @event.Attendees.Add(new EventAttendee { Email = email });
                }

                var request = calendar.Events.Update(@event, tenantCalendarId, @event.Id);
                request.SendNotifications = true;

                request.Execute();
            };

            try
            {
                Retry.Do(addParticipants, TimeSpan.FromMilliseconds(500), 3);
            }
            catch (AggregateException e)
            {
                _logger.Error(e);
            }
        }

        public void RemoveParticipants(Guid eventId, IEnumerable<string> emails, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            Action removeParticipants = () =>
            {
                var @event = GetEvent(eventId, tenantCalendarId);
                if (@event == null)
                {
                    return;
                }

                if (@event.Attendees == null)
                {
                    @event.Attendees = new List<EventAttendee>();
                }

                var attendees = @event
                    .Attendees
                    .Where(a => emails.Contains(a.Email))
                    .ToList();

                foreach (var attendee in attendees)
                {
                    @event.Attendees.Remove(attendee);
                }

                var request = calendar.Events.Update(@event, tenantCalendarId, @event.Id);
                request.SendNotifications = true;

                request.Execute();
            };

            try
            {
                Retry.Do(removeParticipants, TimeSpan.FromMilliseconds(500), 3);
            }
            catch (AggregateException e)
            {
                _logger.Error(e);
            }
        }

        public void ResetParticipants(Guid eventId, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return;
            }

            Action resetParticipants = () =>
            {
                var @event = GetEvent(eventId, tenantCalendarId);
                if (@event?.Attendees == null)
                {
                    return;
                }

                var eventAttendees = @event.Attendees.ToList();
                foreach (var attendee in eventAttendees)
                {
                    @event.Attendees.Remove(attendee);
                }

                var request = calendar.Events.Update(@event, tenantCalendarId, @event.Id);
                request.SendNotifications = true;

                request.Execute();
            };

            try
            {
                Retry.Do(resetParticipants, TimeSpan.FromMilliseconds(500), 3);
            }
            catch (AggregateException e)
            {
                _logger.Error(e);
            }
        }

        private static string ConvertGuidToFormattedString(Guid eventId) => eventId.ToString(EventIdStringFormat);

        private static EventDateTime ParseToEventDateTime(DateTime dt) => new EventDateTime { DateTime = dt };

        private Event GetEvent(Guid eventId, string tenantCalendarId)
        {
            var calendar = CreateCalendarService();
            if (calendar == null)
            {
                return null;
            }

            Event @event;
            var formattedEventId = ConvertGuidToFormattedString(eventId);
            var getEventRequest = calendar.Events.Get(tenantCalendarId, formattedEventId);
            try
            {
                @event = getEventRequest.Execute();
            }
            catch (GoogleApiException e)
            {
                _logger.Error(e);
                return null;
            }

            return @event;
        }

        private CalendarService CreateCalendarService()
        {
            if (string.IsNullOrEmpty(_appSettings.GoogleCalendarServiceCertRelativePath) || string.IsNullOrEmpty(_appSettings.GoogleCalendarServiceCertPassword))
            {
                return null;
            }

            var certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _appSettings.GoogleCalendarServiceCertRelativePath);

            ServiceAccountCredential credential;
            try
            {
                var cert = new X509Certificate2(certPath, _appSettings.GoogleCalendarServiceCertPassword, X509KeyStorageFlags.Exportable);

                credential = new ServiceAccountCredential(new ServiceAccountCredential
                    .Initializer(_appSettings.GoogleCalendarServiceId)
                    {
                        Scopes = new[] { CalendarService.Scope.Calendar }
                    }.FromCertificate(cert));
            }
            catch (CryptographicException e)
            {
                _logger.Error(e);
                return null;
            }

            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SimoonaApp",
            });
        }
    }
}