using AutoMapper;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
            CreateMap<OfficeDTO, EventOfficeViewModel>();
            CreateMap<EventTypeDTO, EventTypeViewModel>();
            CreateMap<EventListItemDTO, EventListItemViewModel>();
            CreateMap<EventOptionDTO, EventOptionViewModel>();
            CreateMap<EventDetailsDTO, EventDetailsViewModel>()
                .Ignore(x => x.Comments);
            CreateMap<EventDetailsOptionDTO, EventDetailsOptionViewModel>();
            CreateMap<EventDetailsParticipantDTO, EventDetailsParticipantViewModel>();
            CreateMap<EventEditDTO, EventEditViewModel>();
            CreateMap<EventOptionsDTO, EventOptionsViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateEventViewModel, CreateEventDto>()
                .Ignore(d => d.ResetParticipantList)
                .Ignore(d => d.Id)
                .IgnoreUserOrgDto();
            CreateMap<UpdateEventViewModel, EditEventDTO>()
                .IgnoreUserOrgDto();
            CreateMap<MyEventsOptionsViewModel, MyEventsOptionsDTO>()
                .IgnoreUserOrgDto();
            CreateMap<EventJoinViewModel, EventJoinDTO>()
                .Ignore(d => d.ParticipantIds)
                .IgnoreUserOrgDto();
            CreateMap<EventJoinMultipleViewModel, EventJoinDTO>()
                .IgnoreUserOrgDto();
            CreateMap<EventOptionViewModel, EventOptionDTO>();

            CreateMap<CreateEventTypeViewModel, CreateEventTypeDTO>().IgnoreUserOrgDto();
            CreateMap<UpdateEventTypeViewModel, UpdateEventTypeDTO>().IgnoreUserOrgDto();
        }

        private void CreateEventsModelMappings()
        {
            CreateMap<Event, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipants, opt => opt.Ignore());
        }
    }
}
