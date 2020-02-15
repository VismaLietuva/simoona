using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Presentation.WebViewModels.Map;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class OfficeMapProfile : Profile
    {
        protected override void Configure()
        {
            CreateOfficeMapMappings();
            CreateMapViewModelMappings();
        }

        private void CreateOfficeMapMappings()
        {
            CreateMap<ApplicationUser, OfficeUserDTO>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));

            CreateMap<Room, OfficeRoomDTO>();

            CreateMap<Office, OfficeDTO>();
        }

        private void CreateMapViewModelMappings()
        {
            CreateMap<Room, MapRoomViewModel>();
            CreateMap<Floor, MapFloorViewModel>()
                .ForMember(dest => dest.OrganizationName, src => src.MapFrom(f => f.Organization.ShortName));
            CreateMap<Floor, MapAllFloorsViewModel>();
            CreateMap<Office, MapOfficeViewModel>();
            CreateMap<RoomType, MapRoomTypeViewModel>();
            CreateMap<ApplicationUser, MapApplicationUserViewModel>();
        }
    }
}
