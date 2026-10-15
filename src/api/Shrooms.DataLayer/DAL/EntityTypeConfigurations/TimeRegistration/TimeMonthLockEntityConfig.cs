using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeMonthLockEntityConfig : IEntityTypeConfiguration<TimeMonthLock>
    {
        public void Configure(EntityTypeBuilder<TimeMonthLock> builder)
        {
            builder.ToTable("TimeMonthLocks");

            builder.Property(x => x.LockedById).IsRequired();
            builder.Property(x => x.Month).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Month })
                .IsUnique()
                .HasDatabaseName("IX_TimeMonthLocks_OrganizationId_Month");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LockedBy)
                .WithMany()
                .HasForeignKey(x => x.LockedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
