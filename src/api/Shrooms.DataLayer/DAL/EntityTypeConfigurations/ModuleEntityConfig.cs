using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ModuleEntityConfig : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.Ignore(m => m.IsDeleted);

            builder.HasMany(m => m.Organizations)
                .WithMany(o => o.ShroomsModules)
                .UsingEntity<Dictionary<string, object>>(
                    "ModuleOrganizations",
                    j => j.HasOne<Organization>().WithMany().HasForeignKey("Organization_Id"),
                    j => j.HasOne<Module>().WithMany().HasForeignKey("Module_Id"));
        }
    }
}
