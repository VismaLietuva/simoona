using AutoMapper;
using Shrooms.EntityModels.Models;

namespace Shrooms.ModelMappings.Resolvers
{
    public class AdministrationUserRoomResolver : ValueResolver<ApplicationUser, bool>
    {
        protected override bool ResolveCore(ApplicationUser source)
        {
            return source.RoomId != null && !(source.RoomId <= 0);
        }
    }
}