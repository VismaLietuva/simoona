using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.TimeRegistration
{
    internal class TimeCustomerEntityConfig : IEntityTypeConfiguration<TimeCustomer>
    {
        public void Configure(EntityTypeBuilder<TimeCustomer> builder)
        {
            builder.ToTable("TimeCustomers");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(TimeCustomer.MaxNameLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_TimeCustomers_OrganizationId_Name");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
