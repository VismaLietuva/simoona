using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Presentation.ModelMappings.Resolvers
{
    public class AdministrationUserRoomResolver : IValueResolver<ApplicationUser, object, bool>
    {
        public bool Resolve(ApplicationUser source, object destination, bool destMember, ResolutionContext context)
        {
            return source.RoomId != null && !(source.RoomId <= 0);
        }
    }
}