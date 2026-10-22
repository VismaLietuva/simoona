using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.FeatureFlags;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class FeatureFlagUserEntityConfig : IEntityTypeConfiguration<FeatureFlagUser>
    {
        public void Configure(EntityTypeBuilder<FeatureFlagUser> builder)
        {
            builder.ToTable("FeatureFlagUsers");

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.FeatureFlagId, x.UserId })
                .IsUnique()
                .HasDatabaseName("IX_FeatureFlagUsers_FeatureFlagId_UserId");

            builder.HasOne(x => x.FeatureFlag)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
