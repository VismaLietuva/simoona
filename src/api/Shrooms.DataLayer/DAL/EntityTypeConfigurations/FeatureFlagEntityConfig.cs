using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.FeatureFlags;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class FeatureFlagEntityConfig : IEntityTypeConfiguration<FeatureFlag>
    {
        public void Configure(EntityTypeBuilder<FeatureFlag> builder)
        {
            builder.ToTable("FeatureFlags");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(FeatureFlag.MaxNameLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique()
                .HasDatabaseName("IX_FeatureFlags_OrganizationId_Name");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
