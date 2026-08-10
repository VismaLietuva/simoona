using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Emoji;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class CustomEmojiConfig : IEntityTypeConfiguration<CustomEmoji>
    {
        public void Configure(EntityTypeBuilder<CustomEmoji> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);

            builder.Property(x => x.BlobName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.Created).HasColumnType("datetime2");

            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique()
                .HasDatabaseName("IX_CustomEmojis_OrganizationId_Name");

            builder.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
