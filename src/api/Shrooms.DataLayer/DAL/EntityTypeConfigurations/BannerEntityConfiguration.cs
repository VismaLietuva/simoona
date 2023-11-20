using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class BannerEntityConfiguration : EntityTypeConfiguration<Banner>
    {
        public BannerEntityConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            Property(e => e.PictureId)
                .IsRequired();

            Property(e => e.ValidFrom)
                .HasColumnType("datetime2");

            Property(e => e.ValidTo)
                .HasColumnType("datetime2");
        }
    }
}
