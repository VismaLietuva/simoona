using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Emoji;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class CustomEmojiConfig : IEntityTypeConfiguration<CustomEmoji>
    {
        public void Configure(EntityTypeBuilder<CustomEmoji> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);

            builder.Property(x => x.BlobName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.AuthorId).IsRequired();

            builder.Property(x => x.Created).HasColumnType("datetime2");

            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_CustomEmojis_OrganizationId_Name");

            builder.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
