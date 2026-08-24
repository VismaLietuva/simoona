using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Books;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class BookLogEntityConfig : IEntityTypeConfiguration<BookLog>
    {
        public void Configure(EntityTypeBuilder<BookLog> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(u => u.BookOffice)
               .WithMany(x => x.BookLogs)
               .HasForeignKey(x => x.BookOfficeId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Organization)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.BookLogs)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.TakenFrom)
                .IsRequired();

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

        }
    }
}
