using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeProjectMemberEntityConfig : IEntityTypeConfiguration<TimeProjectMember>
    {
        public void Configure(EntityTypeBuilder<TimeProjectMember> builder)
        {
            builder.ToTable("TimeProjectMembers");

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.ProjectId, x.UserId })
                .IsUnique()
                .HasDatabaseName("IX_TimeProjectMembers_ProjectId_UserId");

            builder.HasIndex(x => new { x.OrganizationId, x.UserId })
                .HasDatabaseName("IX_TimeProjectMembers_OrganizationId_UserId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
