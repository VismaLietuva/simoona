using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallModeratorsConfiguration : EntityTypeConfiguration<WallModerator>
    {
        public WallModeratorsConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}