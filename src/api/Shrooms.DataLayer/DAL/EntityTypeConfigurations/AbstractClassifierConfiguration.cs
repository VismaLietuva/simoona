using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class AbstractClassifierConfiguration : IEntityTypeConfiguration<AbstractClassifier>
    {
        public void Configure(EntityTypeBuilder<AbstractClassifier> builder)
        {
            builder.HasDiscriminator<string>("ClassificatorType")
                .HasValue<Language>("Language")
                .HasValue<Certificate>("Certificate");

            builder.HasQueryFilter(m => !m.IsDeleted);

            builder.HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
