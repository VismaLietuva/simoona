// This configuration is no longer needed in EF Core with ASP.NET Core Identity
// The IdentityDbContext in Microsoft.AspNetCore.Identity.EntityFrameworkCore automatically configures these tables
// Keeping this file for reference but it's not used

/*
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class IdentityUserLoginEntityConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
        {
            builder.HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });
            builder.ToTable("AspNetUserLogins");
        }
    }
}
*/
