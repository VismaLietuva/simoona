using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeLogEntityConfiguration : IEntityTypeConfiguration<BadgeLog>
    {
        public void Configure(EntityTypeBuilder<BadgeLog> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

            builder.Property(log => log.OrganizationId)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany(x => x.BadgeLogs)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
