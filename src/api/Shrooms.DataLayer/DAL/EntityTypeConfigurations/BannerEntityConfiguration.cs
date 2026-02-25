using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class BannerEntityConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            // Soft delete query filter
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.PictureId)
                .IsRequired();

            builder.Property(e => e.ValidFrom)
                .HasColumnType("datetime2");

            builder.Property(e => e.ValidTo)
                .HasColumnType("datetime2");
        }
    }
}
