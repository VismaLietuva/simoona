using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class OfficeMapProfile : Profile
    {
        protected override void Configure()
        {
            CreateOfficeMapMappings();
        }

        private void CreateOfficeMapMappings()
        {
            CreateMap<ApplicationUser, OfficeUserDTO>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));

            CreateMap<Room, OfficeRoomDTO>();

            CreateMap<Office, OfficeDTO>();
        }
    }
}
