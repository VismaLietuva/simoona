using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeEntryEntityConfig : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("TimeEntries");

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.UserId, x.TaskId, x.Date })
                .IsUnique()
                .HasDatabaseName("IX_TimeEntries_UserId_TaskId_Date");

            builder.HasIndex(x => new { x.OrganizationId, x.UserId, x.Date })
                .HasDatabaseName("IX_TimeEntries_OrganizationId_UserId_Date");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Task)
                .WithMany()
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
