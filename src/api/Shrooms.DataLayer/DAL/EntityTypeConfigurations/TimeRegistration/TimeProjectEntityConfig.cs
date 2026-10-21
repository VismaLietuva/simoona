using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeProjectEntityConfig : IEntityTypeConfiguration<TimeProject>
    {
        public void Configure(EntityTypeBuilder<TimeProject> builder)
        {
            builder.ToTable("TimeProjects");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(TimeProject.MaxNameLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.CustomerId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_TimeProjects_CustomerId_Name");

            builder.HasIndex(x => new { x.OrganizationId, x.IsArchived })
                .HasDatabaseName("IX_TimeProjects_OrganizationId_IsArchived");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
