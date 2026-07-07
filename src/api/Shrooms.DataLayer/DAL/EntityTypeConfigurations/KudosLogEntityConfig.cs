using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class KudosLogEntityConfig : IEntityTypeConfiguration<KudosLog>
    {
        public void Configure(EntityTypeBuilder<KudosLog> builder)
        {
            builder.Property(log => log.Comments)
                .IsRequired();

            builder.Property(log => log.RejectionMessage)
                .IsRequired(false);

            builder.Property(log => log.OrganizationId)
                .IsRequired();

            builder.Property(log => log.PictureId)
                .IsRequired(false);
        }
    }
}
