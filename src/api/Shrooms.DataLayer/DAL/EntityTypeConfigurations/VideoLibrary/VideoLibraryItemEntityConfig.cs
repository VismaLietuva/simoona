using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.VideoLibrary
{
    internal class VideoLibraryItemEntityConfig : IEntityTypeConfiguration<VideoLibraryItem>
    {
        public void Configure(EntityTypeBuilder<VideoLibraryItem> builder)
        {
            builder.ToTable("VideoLibraryItems");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(VideoLibraryItem.MaxTitleLength);
            builder.Property(x => x.Url).IsRequired().HasMaxLength(VideoLibraryItem.MaxUrlLength);
            builder.Property(x => x.Description).HasMaxLength(VideoLibraryItem.MaxDescriptionLength);
            builder.Property(x => x.PictureId).HasMaxLength(VideoLibraryItem.MaxPictureIdLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Created })
                .HasDatabaseName("IX_VideoLibraryItems_OrganizationId_Created");

            builder.HasIndex(x => new { x.OrganizationId, x.VideoTypeId })
                .HasDatabaseName("IX_VideoLibraryItems_OrganizationId_VideoTypeId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VideoType)
                .WithMany(x => x.Videos)
                .HasForeignKey(x => x.VideoTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
