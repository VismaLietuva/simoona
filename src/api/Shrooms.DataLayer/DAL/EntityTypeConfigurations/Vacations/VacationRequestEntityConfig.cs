using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class VacationRequestEntityConfig : IEntityTypeConfiguration<VacationRequest>
    {
        public void Configure(EntityTypeBuilder<VacationRequest> builder)
        {
            builder.ToTable("VacationRequests");

            builder.Property(x => x.EmployeeId).IsRequired();

            // Calendar days, not instants: a "date" column cannot pick up an
            // offset on the way in or out.
            builder.Property(x => x.DateFrom).HasColumnType("date");
            builder.Property(x => x.DateTo).HasColumnType("date");

            builder.Property(x => x.Note).HasMaxLength(VacationRequest.MaxNoteLength);
            builder.Property(x => x.ReviewComment).HasMaxLength(VacationRequest.MaxReviewCommentLength);

            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");
            builder.Property(x => x.ReviewedAt).HasColumnType("datetime2");

            // Balance and overlap checks both filter on exactly this triple.
            builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.Status })
                .HasDatabaseName("IX_VacationRequests_OrganizationId_EmployeeId_Status");

            // The register's date-range filter.
            builder.HasIndex(x => new { x.OrganizationId, x.DateFrom })
                .HasDatabaseName("IX_VacationRequests_OrganizationId_DateFrom");

            // The register's default ordering. Without it every page of the
            // organisation-wide list sorts the whole table to return ten rows.
            builder.HasIndex(x => new { x.OrganizationId, x.Created })
                .HasDatabaseName("IX_VacationRequests_OrganizationId_Created");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedBy)
                .WithMany()
                .HasForeignKey(x => x.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
