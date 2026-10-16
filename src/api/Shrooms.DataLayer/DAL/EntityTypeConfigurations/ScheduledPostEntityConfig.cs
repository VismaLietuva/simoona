using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ScheduledPostEntityConfig : IEntityTypeConfiguration<ScheduledPost>
    {
        public void Configure(EntityTypeBuilder<ScheduledPost> builder)
        {
            builder.ToTable("ScheduledPosts");

            builder.Property(x => x.JobId).HasMaxLength(ScheduledPost.MaxJobIdLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");
            builder.Property(x => x.ScheduledFor).HasColumnType("datetime2");
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.OwnsOne(x => x.Images, b => b.Property(x => x.Serialized).HasColumnName("Images"));

            builder.HasIndex(x => new { x.OrganizationId, x.AuthorId, x.WallId })
                .HasDatabaseName("IX_ScheduledPosts_OrganizationId_AuthorId_WallId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wall)
                .WithMany()
                .HasForeignKey(x => x.WallId)
                .OnDelete(DeleteBehavior.ClientNoAction);
        }
    }
}
