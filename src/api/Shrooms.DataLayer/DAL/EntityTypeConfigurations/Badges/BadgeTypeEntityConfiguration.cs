using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeTypeEntityConfiguration : IEntityTypeConfiguration<BadgeType>
    {
        public void Configure(EntityTypeBuilder<BadgeType> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(x => x.BadgeCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Description)
                .HasMaxLength(4000);

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);
        }
    }
}
