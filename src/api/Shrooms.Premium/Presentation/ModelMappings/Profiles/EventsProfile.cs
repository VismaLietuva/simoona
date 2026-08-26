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
        public EventsProfile()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEventsModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<OfficeDto, EventOfficeViewModel>(MemberList.None);
            CreateMap<EventTypeDto, EventTypeViewModel>(MemberList.None);
            CreateMap<EventListItemDto, EventListItemViewModel>(MemberList.None)
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventDetailsListItemDto, EventDetailsListItemViewModel>(MemberList.None);
            CreateMap<FoodTeamEventDto, FoodTeamEventViewModel>(MemberList.None);
            CreateMap<FoodTeamWidgetDto, FoodTeamWidgetViewModel>(MemberList.None);
            CreateMap<EventOptionDto, EventOptionViewModel>(MemberList.None);
            CreateMap<EventDetailsDto, EventDetailsViewModel>(MemberList.None)
                .Ignore(x => x.Comments)
                .Ignore(x => x.IsForAllOffices)
                .ForMember(dest => dest.OfficesName, opt => opt.MapFrom(u => u.Offices.OfficeNames));

            CreateMap<NewEventOptionDto, NewEventOptionViewModel>(MemberList.None);
            CreateMap<NewEventOptionViewModel, NewEventOptionDto>(MemberList.None);

            CreateMap<EventFilteredArgsViewModel, EventFilteredArgsDto>(MemberList.None)
                .Ignore(opt => opt.TypeIdParsed)
                .Ignore(opt => opt.OfficeIdParsed);

            CreateMap<EventDetailsOptionDto, EventDetailsOptionViewModel>(MemberList.None);
            CreateMap<EventDetailsParticipantDto, EventDetailsParticipantViewModel>(MemberList.None);
            CreateMap<EventVisitedReportDto, EventVisitedReportViewModel>(MemberList.None);
            CreateMap<EventProjectReportDto, EventProjectReportViewModel>(MemberList.None);
            CreateMap<EventParticipantReportDto, EventParticipantReportViewModel>(MemberList.None);

            CreateMap<EventEditDetailsDto, EventEditDetailsViewModel>(MemberList.None)
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventOptionsDto, EventOptionsViewModel>(MemberList.None);

            // Read side. Id is non-null on anything that came out of the database, so the
            // nullable write-side Id is flattened here rather than leaking a null to the client.
            CreateMap<EventQuestionStructureDto, EventSignUpQuestionViewModel>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            CreateMap<EventQuestionOptionStructureDto, EventSignUpOptionViewModel>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            CreateMap<EventQuestionStructureDto, EventQuestionViewModel>()
                .ForMember(dest => dest.ShowIf, opt => opt.MapFrom(src =>
                    src.ShowIfOptionId == null && src.ShowIfOptionClientId == null
                        ? null
                        : new EventQuestionConditionViewModel
                        {
                            OptionId = src.ShowIfOptionId,
                            OptionClientId = src.ShowIfOptionClientId
                        }));

            CreateMap<EventChangeOptionViewModel, EventChangeOptionsDto>(MemberList.None)
                .Ignore(x => x.OrganizationId)
                .Ignore(x => x.UserId);

            CreateMap<EventOfficesDto, EventOfficesViewModel>(MemberList.None);
            CreateMap<EventReportDetailsDto, EventReportDetailsViewModel>(MemberList.None);

            CreateMap<EventReminderDto, EventReminderViewModel>(MemberList.None);
            CreateMap<EventReminderDetailsDto, EventReminderDetailsViewModel>(MemberList.None)
                .ForMember(dest => dest.IsDisabled, opt => opt.MapFrom(u => u.RemindedCount > 0));
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateEventViewModel, CreateEventDto>(MemberList.None)
                .Ignore(d => d.Id)
                .Ignore(d => d.Offices)
                .IgnoreUserOrgDto();
            CreateMap<UpdateEventViewModel, EditEventDto>(MemberList.None)
                .IgnoreUserOrgDto()
                .Ignore(d => d.Offices);
            CreateMap<MyEventsOptionsViewModel, MyEventsOptionsDto>(MemberList.None);
            CreateMap<EventSearchOptionsViewModel, EventSearchOptionsDto>(MemberList.None);
            CreateMap<EventJoinViewModel, EventJoinDto>(MemberList.None)
                .Ignore(d => d.ParticipantIds)
                .IgnoreUserOrgDto();
            CreateMap<EventJoinMultipleViewModel, EventJoinDto>(MemberList.None)
                .Ignore(d => d.AttendComment)
                .IgnoreUserOrgDto();
            CreateMap<EventOptionViewModel, EventOptionDto>(MemberList.None);

            CreateMap<EventQuestionOptionViewModel, EventQuestionOptionStructureDto>().ReverseMap();

            CreateMap<EventQuestionViewModel, EventQuestionStructureDto>()
                .ForMember(dest => dest.ShowIfOptionId,
                    opt => opt.MapFrom(src => src.ShowIf == null ? (int?)null : src.ShowIf.OptionId))
                .ForMember(dest => dest.ShowIfOptionClientId,
                    opt => opt.MapFrom(src => src.ShowIf == null ? null : src.ShowIf.OptionClientId));

            CreateMap<UpdateAttendStatusViewModel, UpdateAttendStatusDto>(MemberList.None)
                .IgnoreUserOrgDto();

            CreateMap<EventDetailsOptionViewModel, EventDetailsOptionDto>(MemberList.None);
            CreateMap<EventDetailsParticipantViewModel, EventDetailsParticipantDto>(MemberList.None);
            CreateMap<EventVisitedReportViewModel, EventVisitedReportDto>(MemberList.None);
            CreateMap<EventProjectReportViewModel, EventProjectReportDto>(MemberList.None);
            CreateMap<EventParticipantReportViewModel, EventParticipantReportDto>(MemberList.None);

            CreateMap<CreateEventTypeViewModel, CreateEventTypeDto>(MemberList.None).IgnoreUserOrgDto();
            CreateMap<UpdateEventTypeViewModel, UpdateEventTypeDto>(MemberList.None).IgnoreUserOrgDto();
            CreateMap<EventParticipantsReportListingArgsViewModel, EventParticipantsReportListingArgsDto>(MemberList.None);
            CreateMap<EventReportListingArgsViewModel, EventReportListingArgsDto>(MemberList.None);
            CreateMap<EventParticipantVisitedEventsListingArgsViewModel, EventParticipantVisitedEventsListingArgsDto>(MemberList.None);
            CreateMap<EventReminderViewModel, EventReminderDto>(MemberList.None);
        }

        private void CreateEventsModelMappings()
        {
            CreateMap<Event, Event>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipants, opt => opt.Ignore());
        }
    }
}
