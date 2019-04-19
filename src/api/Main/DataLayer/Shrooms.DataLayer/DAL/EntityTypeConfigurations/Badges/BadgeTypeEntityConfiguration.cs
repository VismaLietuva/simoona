using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeTypeEntityConfiguration : EntityTypeConfiguration<BadgeType>
    {
        public BadgeTypeEntityConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(x => x.BadgeCategory)
                .WithMany()
                .WillCascadeOnDelete(false);

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
