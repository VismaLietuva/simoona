using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.VideoLibrary
{
    internal class VideoTypeEntityConfig : IEntityTypeConfiguration<VideoType>
    {
        public void Configure(EntityTypeBuilder<VideoType> builder)
        {
            builder.ToTable("VideoTypes");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(VideoType.MaxTitleLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Title })
                .HasDatabaseName("IX_VideoTypes_OrganizationId_Title");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
