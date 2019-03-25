using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class OrganizationEntityConfig : EntityTypeConfiguration<Organization>
    {
        public OrganizationEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}
