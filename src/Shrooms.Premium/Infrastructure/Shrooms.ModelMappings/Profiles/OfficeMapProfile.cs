using AutoMapper;
using Shrooms.EntityModels.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
