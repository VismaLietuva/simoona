using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeLogEntityConfiguration : EntityTypeConfiguration<BadgeLog>
    {
        public BadgeLogEntityConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            Property(u => u.CreatedBy)
                .HasMaxLength(50);

            Property(log => log.OrganizationId)
                .IsRequired();

            HasRequired(x => x.Employee)
                .WithMany(x => x.BadgeLogs)
                .WillCascadeOnDelete(false);
        }
    }
}
