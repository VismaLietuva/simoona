using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ExternalLinkConfig : EntityTypeConfiguration<ExternalLink>
    {
        public ExternalLinkConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            Property(x => x.Name).IsRequired();

            Property(x => x.Url).IsRequired();

            Property(x => x.Type).IsRequired();

            Property(x => x.Created).HasColumnType("datetime2");

            Property(x => x.Modified).HasColumnType("datetime2");

            HasRequired(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}
