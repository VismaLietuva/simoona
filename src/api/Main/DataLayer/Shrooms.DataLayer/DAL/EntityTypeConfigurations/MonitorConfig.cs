using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Monitors;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class MonitorConfig : EntityTypeConfiguration<Monitor>
    {
        public MonitorConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(x => x.Organization)
              .WithMany()
              .WillCascadeOnDelete(false);
        }
    }
}
