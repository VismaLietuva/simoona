using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimePeriodEntityConfig : IEntityTypeConfiguration<TimePeriod>
    {
        public void Configure(EntityTypeBuilder<TimePeriod> builder)
        {
            builder.ToTable("TimePeriods");

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Month).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.UserId, x.Month })
                .IsUnique()
                .HasDatabaseName("IX_TimePeriods_UserId_Month");

            builder.HasIndex(x => new { x.OrganizationId, x.Month })
                .HasDatabaseName("IX_TimePeriods_OrganizationId_Month");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
