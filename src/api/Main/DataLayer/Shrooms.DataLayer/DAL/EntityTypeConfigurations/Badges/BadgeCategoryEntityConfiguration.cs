using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeCategoryEntityConfiguration : EntityTypeConfiguration<BadgeCategory>
    {
        public BadgeCategoryEntityConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            Property(u => u.Title)
                .IsRequired()
                .HasMaxLength(50);

            Property(u => u.Description)
                .HasMaxLength(4000);

            Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            Property(u => u.CreatedBy)
                .HasMaxLength(50);
        }
    }
}
