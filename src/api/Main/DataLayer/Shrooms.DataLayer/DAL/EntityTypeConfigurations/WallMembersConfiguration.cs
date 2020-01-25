using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallMembersConfiguration : EntityTypeConfiguration<WallMember>
    {
        public WallMembersConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}
