using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Books;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class BookOfficeEntityConfig : IEntityTypeConfiguration<BookOffice>
    {
        public void Configure(EntityTypeBuilder<BookOffice> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(u => u.Book)
                .WithMany(x => x.BookOffices)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Office)
                .WithMany(x => x.BookOffices)
                .HasForeignKey(x => x.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

            builder.HasIndex(u => new { u.BookId, u.OfficeId })
                .IsUnique()
                .HasDatabaseName("BookId_OfficeId");
        }
    }
}
