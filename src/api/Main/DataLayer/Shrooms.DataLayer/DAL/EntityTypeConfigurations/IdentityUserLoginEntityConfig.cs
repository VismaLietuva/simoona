using System.Data.Entity.ModelConfiguration;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class IdentityUserLoginEntityConfig : EntityTypeConfiguration<IdentityUserLogin>
    {
        public IdentityUserLoginEntityConfig()
        {
            HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");
        }
    }
}
