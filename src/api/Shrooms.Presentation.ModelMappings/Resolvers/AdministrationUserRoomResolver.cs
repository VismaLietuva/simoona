using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Presentation.ModelMappings.Resolvers
{
    public class AdministrationUserRoomResolver : ValueResolver<ApplicationUser, bool>
    {
        protected override bool ResolveCore(ApplicationUser source)
        {
            return source.RoomId != null && !(source.RoomId <= 0);
        }
    }
}