using AutoMapper;
using Newtonsoft.Json;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DataTransferObjects.Models.OfficeMap;
using Shrooms.EntityModels.Models.Events;
using Shrooms.WebViewModels.Models.Events;
using System.Linq;

namespace Shrooms.ModelMappings.Profiles
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
            CreateMap<EventListItemDTO, EventListItemViewModel>()
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventOptionDTO, EventOptionViewModel>();
            CreateMap<EventDetailsDTO, EventDetailsViewModel>()
                .Ignore(x => x.Comments)
                .ForMember(dest => dest.OfficesName, opt => opt.MapFrom(u => u.Offices.OfficeNames));

            CreateMap<EventDetailsOptionDTO, EventDetailsOptionViewModel>();
            CreateMap<EventDetailsParticipantDTO, EventDetailsParticipantViewModel>();
            CreateMap<EventEditDTO, EventEditViewModel>()
                .ForMember(dest => dest.OfficeIds, opt => opt.MapFrom(u => JsonConvert.DeserializeObject<string[]>(u.Offices.Value)));
            CreateMap<EventOptionsDTO, EventOptionsViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateEventViewModel, CreateEventDto>()
                .Ignore(d => d.ResetParticipantList)
                .Ignore(d => d.Id)
                .Ignore(d => d.Offices)
                .IgnoreUserOrgDto();
            CreateMap<UpdateEventViewModel, EditEventDTO>()
                .IgnoreUserOrgDto()
                .Ignore(d => d.Offices);
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
