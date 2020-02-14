using System.Data.Entity.ModelConfiguration;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class IdentityUserClaimEntityConfig : EntityTypeConfiguration<IdentityUserClaim>
    {
        public IdentityUserClaimEntityConfig()
        {
            ToTable("AspNetUserClaims");
        }
    }
}
