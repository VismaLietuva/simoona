using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.API.GeneralCode.SerializationIgnorer
{
    public static class SerializationIgnoreConfigs
    {
        public static void Configure()
        {
            SerializationIgnorer.Create<ApplicationUserViewModel>()
                .ForMember<QualificationLevelViewModel>(e => e.ApplicationUsers)
                .ForMember<RoomViewModel>(e => e.ApplicationUsers);

            SerializationIgnorer.Create<ApplicationUserViewPagedModel>()
                .ForMember<QualificationLevelViewModel>(e => e.ApplicationUsers)
                .ForMember<RoomViewModel>(e => e.ApplicationUsers);

            SerializationIgnorer.Create<OfficeViewModel>()
                .ForMember<FloorViewModel>(e => e.Office);

            SerializationIgnorer.Create<FloorViewModel>()
                .ForMember<OfficeViewModel>(e => e.Floors)
                .ForMember<RoomViewModel>(e => e.Floor);

            SerializationIgnorer.Create<QualificationLevelViewModel>()
                .ForMember<ApplicationUserViewModel>(e => e.QualificationLevel);

            SerializationIgnorer.Create<RoomViewModel>()
                .ForMember<FloorViewModel>(e => e.Rooms)
                .ForMember<ApplicationUserViewModel>(e => e.Room);
        }
    }
}