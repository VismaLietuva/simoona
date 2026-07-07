using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ExternalLinkConfig : IEntityTypeConfiguration<ExternalLink>
    {
        public void Configure(EntityTypeBuilder<ExternalLink> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Name).IsRequired();

            builder.Property(x => x.Url).IsRequired();

            builder.Property(x => x.Type).IsRequired();

            builder.Property(x => x.Created).HasColumnType("datetime2");

            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
