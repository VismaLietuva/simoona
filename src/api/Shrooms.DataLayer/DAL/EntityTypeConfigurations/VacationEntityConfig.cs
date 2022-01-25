using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class VacationEntityConfig : EntityTypeConfiguration<VacationPage>
    {
        public VacationEntityConfig()
        {
            Property(v => v.Content)
                .IsRequired();
        }
    }
}