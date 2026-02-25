using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Presentation.WebViewModels.Map;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class OfficeMapProfile : Profile
    {
        public OfficeMapProfile()
        {
            CreateOfficeMapMappings();
            CreateMapViewModelMappings();
        }

        private void CreateOfficeMapMappings()
        {
            CreateMap<ApplicationUser, OfficeUserDto>(MemberList.None)
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));

            CreateMap<Room, OfficeRoomDto>(MemberList.None);

            CreateMap<Office, OfficeDto>(MemberList.None);
        }

        private void CreateMapViewModelMappings()
        {
            CreateMap<Room, MapRoomViewModel>(MemberList.None);
            CreateMap<Floor, MapFloorViewModel>(MemberList.None)
                .ForMember(dest => dest.OrganizationName, src => src.MapFrom(f => f.Organization.ShortName));
            CreateMap<Floor, MapAllFloorsViewModel>(MemberList.None);
            CreateMap<Office, MapOfficeViewModel>(MemberList.None);
            CreateMap<RoomType, MapRoomTypeViewModel>(MemberList.None);
            CreateMap<ApplicationUser, MapApplicationUserViewModel>(MemberList.None);
        }
    }
}
