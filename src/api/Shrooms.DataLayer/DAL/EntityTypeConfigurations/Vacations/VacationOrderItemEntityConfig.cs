using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class VacationOrderItemEntityConfig : IEntityTypeConfiguration<VacationOrderItem>
    {
        public void Configure(EntityTypeBuilder<VacationOrderItem> builder)
        {
            builder.ToTable("VacationOrderItems");

            builder.Property(x => x.EmployeeName).IsRequired().HasMaxLength(VacationOrderItem.MaxEmployeeNameLength);

            builder.Property(x => x.DateFrom).HasColumnType("date");
            builder.Property(x => x.DateTo).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasOne(x => x.VacationRequest)
                .WithMany()
                .HasForeignKey(x => x.VacationRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
