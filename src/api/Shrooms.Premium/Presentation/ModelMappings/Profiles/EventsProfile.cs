using AutoMapper;
using Newtonsoft.Json;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
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
            CreateMap<ExtensiveEventDetailsDto, ExtensiveEventDetailsViewModel>();
            CreateMap<EventOptionDto, EventOptionViewModel>();
            CreateMap<EventDetailsDto, EventDetailsViewModel>()
                .Ignore(x => x.Comments)
                .Ignore(x => x.IsForAllOffices)
                .ForMember(dest => dest.OfficesName, opt => opt.MapFrom(u => u.Offices.OfficeNames));

            CreateMap<NewEventOptionDto, NewEventOptionViewModel>();
            CreateMap<NewEventOptionViewModel, NewEventOptionDto>();

            CreateMap<EventDetailsOptionDto, EventDetailsOptionViewModel>();
            CreateMap<EventDetailsParticipantDto, EventDetailsParticipantViewModel>();
            CreateMap<EventVisitedDto, EventVisitedViewModel>();
            CreateMap<EventProjectDto, EventProjectViewModel>();
            CreateMap<ExtensiveEventDetailsDto, ExtensiveEventDetailsViewModel>();
            CreateMap<ExtensiveEventParticipantDto, ExtensiveEventParticipantViewModel>();
            
            CreateMap<EventEditDto, EventEditViewModel>()
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventOptionsDto, EventOptionsViewModel>();

            CreateMap<EventChangeOptionViewModel, EventChangeOptionsDto>()
                .Ignore(x => x.OrganizationId)
                .Ignore(x => x.UserId);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateEventViewModel, CreateEventDto>()
                .Ignore(d => d.ResetParticipantList)
                .Ignore(d => d.Id)
                .Ignore(d => d.Offices)
                .IgnoreUserOrgDto();
            CreateMap<UpdateEventViewModel, EditEventDto>()
                .IgnoreUserOrgDto()
                .Ignore(d => d.Offices);
            CreateMap<MyEventsOptionsViewModel, MyEventsOptionsDto>()
                .IgnoreUserOrgDto();
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
            CreateMap<EventVisitedViewModel, EventVisitedDto>();
            CreateMap<EventProjectViewModel, EventProjectDto>();
            CreateMap<ExtensiveEventDetailsViewModel, ExtensiveEventDetailsDto>();
            CreateMap<ExtensiveEventParticipantViewModel, ExtensiveEventParticipantDto>();

            CreateMap<CreateEventTypeViewModel, CreateEventTypeDto>().IgnoreUserOrgDto();
            CreateMap<UpdateEventTypeViewModel, UpdateEventTypeDto>().IgnoreUserOrgDto();
        }

        private void CreateEventsModelMappings()
        {
            CreateMap<Event, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipants, opt => opt.Ignore());
        }
    }
}
