using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.DAL.Search;
using Shrooms.DataLayer.EntityModels.Models.Search;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class SearchDocumentEntityConfig : IEntityTypeConfiguration<SearchDocument>
    {
        public void Configure(EntityTypeBuilder<SearchDocument> builder)
        {
            builder.ToTable("SearchDocuments");

            // Searches read one organisation's sections at a time, so rows are stored in that
            // order; Id stays the (non-clustered) key the full-text index requires.
            builder.HasKey(x => x.Id).HasName("PK_SearchDocuments").IsClustered(false);

            builder.Property(x => x.Kind).HasColumnType("tinyint");
            builder.Property(x => x.EntityId).IsRequired().HasMaxLength(SearchDocument.EntityIdMaxLength);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(SearchDocument.TitleMaxLength);
            builder.Property(x => x.Description).HasMaxLength(SearchDocument.DescriptionMaxLength);
            builder.Property(x => x.Date).HasColumnType("datetime2");
            builder.Property(x => x.PictureId).HasMaxLength(SearchDocument.EntityIdMaxLength);
            builder.Property(x => x.LinkId).HasMaxLength(SearchDocument.EntityIdMaxLength);
            builder.Property(x => x.AccessKey).HasMaxLength(SearchDocument.AccessKeyMaxLength);
            builder.Property(x => x.OwnerUserId).HasMaxLength(SearchDocument.EntityIdMaxLength);
            builder.Property(x => x.SearchText).IsRequired().HasMaxLength(SearchTextNormalizer.MaxLength).UseCollation(SearchTextNormalizer.Collation);
            builder.Property(x => x.IndexedAt).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Kind, x.EntityId })
                .IsUnique()
                .IsClustered()
                .HasDatabaseName("IX_SearchDocuments_OrganizationId_Kind_EntityId");
        }
    }
}
