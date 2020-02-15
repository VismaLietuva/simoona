using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallConfiguration : EntityTypeConfiguration<Wall>
    {
        public WallConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(x => x.Organization)
               .WithMany()
               .WillCascadeOnDelete(false);
        }
    }
}
