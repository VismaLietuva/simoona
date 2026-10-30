using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class KudosCalculatorSettingsEntityConfig : IEntityTypeConfiguration<KudosCalculatorSettings>
    {
        public void Configure(EntityTypeBuilder<KudosCalculatorSettings> builder)
        {
            builder.ToTable("KudosCalculatorSettings");

            builder.Property(x => x.VatPercent).HasPrecision(5, 2);
            builder.Property(x => x.TaxPercent).HasPrecision(5, 2);
            builder.Property(x => x.TeamMultiplier).HasPrecision(4, 2);
            builder.Property(x => x.CreditPercent).HasPrecision(5, 2);
            builder.Property(x => x.GuideUrl).HasMaxLength(KudosCalculatorSettings.MaxUrlLength);
            builder.Property(x => x.WhitelistUrl).HasMaxLength(KudosCalculatorSettings.MaxUrlLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => x.OrganizationId)
                .IsUnique()
                .HasDatabaseName("IX_KudosCalculatorSettings_OrganizationId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
