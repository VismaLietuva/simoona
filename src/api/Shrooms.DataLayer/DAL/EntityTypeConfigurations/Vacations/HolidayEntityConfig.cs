using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class HolidayEntityConfig : IEntityTypeConfiguration<Holiday>
    {
        public void Configure(EntityTypeBuilder<Holiday> builder)
        {
            builder.ToTable("Holidays");

            builder.HasKey(x => x.Date);

            // No identity: the date is supplied, never generated.
            builder.Property(x => x.Date)
                .HasColumnType("date")
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(Holiday.MaxNameLength);
        }
    }
}
