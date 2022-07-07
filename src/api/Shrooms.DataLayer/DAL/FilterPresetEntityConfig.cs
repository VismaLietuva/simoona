using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity.ModelConfiguration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class FilterPresetEntityConfig : EntityTypeConfiguration<FilterPreset>
    {
        public FilterPresetEntityConfig()
        {
            Map(filter => filter.Requires("IsDeleted").HasValue(false));

            Property(filter => filter.Preset)
                .IsRequired();

            Property(filter => filter.Name)
                .IsRequired();

            Property(filter => filter.ForPage)
                .IsRequired();
        }
    }
}