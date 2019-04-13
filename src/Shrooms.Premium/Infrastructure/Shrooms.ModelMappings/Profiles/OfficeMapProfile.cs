using AutoMapper;
using Shrooms.DataTransferObjects.Models.OfficeMap;
using Shrooms.EntityModels.Models;

namespace Shrooms.ModelMappings.Profiles
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
