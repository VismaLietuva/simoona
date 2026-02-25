using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeCategoryEntityConfiguration : IEntityTypeConfiguration<BadgeCategory>
    {
        public void Configure(EntityTypeBuilder<BadgeCategory> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(u => u.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Description)
                .HasMaxLength(4000);

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

            builder.HasMany(x => x.RelationshipsWithKudosTypes)
                .WithOne()
                .HasForeignKey(x => x.BadgeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.BadgeTypes)
                .WithOne()
                .HasForeignKey(x => x.BadgeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
