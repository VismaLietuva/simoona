using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity.ModelConfiguration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class BlacklistStateEntityConfig : EntityTypeConfiguration<BlacklistState>
    {
        public BlacklistStateEntityConfig()
        {
            Map(e => e.Requires("IsDeleted")
                .HasValue(value: false));

            Property(u => u.Reason)
                .IsOptional();

            Property(u => u.UserId)
                .IsRequired();

            Property(u => u.EndDate)
                .IsRequired();
        }
    }
}
