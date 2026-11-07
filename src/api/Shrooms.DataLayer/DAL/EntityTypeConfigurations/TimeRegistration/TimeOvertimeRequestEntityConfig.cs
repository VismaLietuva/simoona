using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeOvertimeRequestEntityConfig : IEntityTypeConfiguration<TimeOvertimeRequest>
    {
        public void Configure(EntityTypeBuilder<TimeOvertimeRequest> builder)
        {
            builder.ToTable("TimeOvertimeRequests");

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.RequestedById).IsRequired();
            builder.Property(x => x.DateFrom).HasColumnType("date");
            builder.Property(x => x.DateTo).HasColumnType("date");
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(TimeOvertimeRequest.MaxReasonLength);
            builder.Property(x => x.ReviewComment).HasMaxLength(TimeOvertimeRequest.MaxReasonLength);
            builder.Property(x => x.DecidedAt).HasColumnType("datetime2");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.UserId, x.DateFrom })
                .HasDatabaseName("IX_TimeOvertimeRequests_OrganizationId_UserId_DateFrom");

            builder.HasIndex(x => new { x.OrganizationId, x.Status })
                .HasDatabaseName("IX_TimeOvertimeRequests_OrganizationId_Status");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedBy)
                .WithMany()
                .HasForeignKey(x => x.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DecidedBy)
                .WithMany()
                .HasForeignKey(x => x.DecidedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
