using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class KudosLogEntityConfig : EntityTypeConfiguration<KudosLog>
    {
        public KudosLogEntityConfig()
        {
            Property(log => log.Comments)
                .IsRequired();

            Property(log => log.RejectionMessage)
                .IsOptional();

            Property(log => log.OrganizationId)
                .IsRequired();

            Property(log => log.PictureId)
                .IsOptional();
        }
    }
}
