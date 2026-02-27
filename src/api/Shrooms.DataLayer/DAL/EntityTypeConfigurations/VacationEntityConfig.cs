using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class VacationEntityConfig : IEntityTypeConfiguration<VacationPage>
    {
        public void Configure(EntityTypeBuilder<VacationPage> builder)
        {
            builder.Ignore(v => v.IsDeleted);

            builder.Property(v => v.Content)
                .IsRequired();
        }
    }
}