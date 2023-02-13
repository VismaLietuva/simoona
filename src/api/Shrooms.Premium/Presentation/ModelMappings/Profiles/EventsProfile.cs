using AutoMapper;
using Newtonsoft.Json;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Presentation.WebViewModels.Events;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class EventsProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEventsModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<OfficeDto, EventOfficeViewModel>();
            CreateMap<EventTypeDto, EventTypeViewModel>();
            CreateMap<EventListItemDto, EventListItemViewModel>()
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventDetailsListItemDto, EventDetailsListItemViewModel>();
            CreateMap<EventOptionDto, EventOptionViewModel>();
            CreateMap<EventDetailsDto, EventDetailsViewModel>()
                .Ignore(x => x.Comments)
                .Ignore(x => x.IsForAllOffices)
                .ForMember(dest => dest.OfficesName, opt => opt.MapFrom(u => u.Offices.OfficeNames));

            CreateMap<NewEventOptionDto, NewEventOptionViewModel>();
            CreateMap<NewEventOptionViewModel, NewEventOptionDto>();

            CreateMap<EventFilteredArgsViewModel, EventFilteredArgsDto>()
                .Ignore(opt => opt.TypeIdParsed)
                .Ignore(opt => opt.OfficeIdParsed);

            CreateMap<EventDetailsOptionDto, EventDetailsOptionViewModel>();
            CreateMap<EventDetailsParticipantDto, EventDetailsParticipantViewModel>();
            CreateMap<EventVisitedReportDto, EventVisitedReportViewModel>();
            CreateMap<EventProjectReportDto, EventProjectReportViewModel>();
            CreateMap<EventParticipantReportDto, EventParticipantReportViewModel>();

            CreateMap<EventEditDetailsDto, EventEditDetailsViewModel>()
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventOptionsDto, EventOptionsViewModel>();

            CreateMap<EventChangeOptionViewModel, EventChangeOptionsDto>()
                .Ignore(x => x.OrganizationId)
                .Ignore(x => x.UserId);

            CreateMap<EventOfficesDto, EventOfficesViewModel>();
            CreateMap<EventReportDetailsDto, EventReportDetailsViewModel>();

            CreateMap<EventReminderDto, EventReminderViewModel>();
            CreateMap<EventReminderDetailsDto, EventReminderDetailsViewModel>()
                .ForMember(dest => dest.IsDisabled, opt => opt.MapFrom(u => u.RemindedCount > 0));
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateEventViewModel, CreateEventDto>()
                .Ignore(d => d.Id)
                .Ignore(d => d.Offices)
                .IgnoreUserOrgDto();
            CreateMap<UpdateEventViewModel, EditEventDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(u => u.Id.Value.ToString()))
                .IgnoreUserOrgDto()
                .Ignore(d => d.Offices);
            CreateMap<MyEventsOptionsViewModel, MyEventsOptionsDto>();
            CreateMap<EventJoinViewModel, EventJoinDto>()
                .Ignore(d => d.ParticipantIds)
                .IgnoreUserOrgDto();
            CreateMap<EventJoinMultipleViewModel, EventJoinDto>()
                .Ignore(d => d.AttendComment)
                .IgnoreUserOrgDto();
            CreateMap<EventOptionViewModel, EventOptionDto>();

            CreateMap<UpdateAttendStatusViewModel, UpdateAttendStatusDto>()
                .IgnoreUserOrgDto();

            CreateMap<EventDetailsOptionViewModel, EventDetailsOptionDto>();
            CreateMap<EventDetailsParticipantViewModel, EventDetailsParticipantDto>();
            CreateMap<EventVisitedReportViewModel, EventVisitedReportDto>();
            CreateMap<EventProjectReportViewModel, EventProjectReportDto>();
            CreateMap<EventParticipantReportViewModel, EventParticipantReportDto>();

            CreateMap<CreateEventTypeViewModel, CreateEventTypeDto>().IgnoreUserOrgDto();
            CreateMap<UpdateEventTypeViewModel, UpdateEventTypeDto>().IgnoreUserOrgDto();
            CreateMap<EventParticipantsReportListingArgsViewModel, EventParticipantsReportListingArgsDto>();
            CreateMap<EventReportListingArgsViewModel, EventReportListingArgsDto>();
            CreateMap<EventParticipantVisitedEventsListingArgsViewModel, EventParticipantVisitedEventsListingArgsDto>();
            CreateMap<EventReminderViewModel, EventReminderDto>();
        }

        private void CreateEventsModelMappings()
        {
            CreateMap<Event, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipants, opt => opt.Ignore());
        }
    }
}
