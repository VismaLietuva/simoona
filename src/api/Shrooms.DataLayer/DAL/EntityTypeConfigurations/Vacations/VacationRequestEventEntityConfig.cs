using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class VacationRequestEventEntityConfig : IEntityTypeConfiguration<VacationRequestEvent>
    {
        public void Configure(EntityTypeBuilder<VacationRequestEvent> builder)
        {
            builder.ToTable("VacationRequestEvents");

            builder.Property(x => x.ActorId).IsRequired().HasMaxLength(DataLayerConstants.IdentityKeyLength);
            builder.Property(x => x.EmployeeId).IsRequired().HasMaxLength(DataLayerConstants.IdentityKeyLength);

            builder.Property(x => x.DateFrom).HasColumnType("date");
            builder.Property(x => x.DateTo).HasColumnType("date");

            builder.Property(x => x.OccurredAt).HasColumnType("datetime2");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.Property(x => x.Comment).HasMaxLength(VacationRequestEvent.MaxCommentLength);

            // The log's default order, newest first.
            builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt })
                .HasDatabaseName("IX_VacationRequestEvents_OrganizationId_OccurredAt");

            builder.HasIndex(x => x.VacationRequestId)
                .HasDatabaseName("IX_VacationRequestEvents_VacationRequestId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // No cascade: the audit trail outlives the request it describes.
            builder.HasOne(x => x.VacationRequest)
                .WithMany()
                .HasForeignKey(x => x.VacationRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Actor)
                .WithMany()
                .HasForeignKey(x => x.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
