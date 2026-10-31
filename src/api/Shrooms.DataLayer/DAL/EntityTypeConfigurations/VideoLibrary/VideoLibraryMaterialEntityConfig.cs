using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.VideoLibrary
{
    internal class VideoLibraryMaterialEntityConfig : IEntityTypeConfiguration<VideoLibraryMaterial>
    {
        public void Configure(EntityTypeBuilder<VideoLibraryMaterial> builder)
        {
            builder.ToTable("VideoLibraryMaterials");

            builder.HasQueryFilter(e => !e.VideoLibraryItem.IsDeleted);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(VideoLibraryMaterial.MaxTitleLength);
            builder.Property(x => x.Url).IsRequired().HasMaxLength(VideoLibraryMaterial.MaxUrlLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.VideoLibraryItemId, x.SortOrder })
                .HasDatabaseName("IX_VideoLibraryMaterials_VideoLibraryItemId_SortOrder");

            builder.HasOne(x => x.VideoLibraryItem)
                .WithMany(x => x.Materials)
                .HasForeignKey(x => x.VideoLibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
