using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Books;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class BookEntityConfig : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(x => x.Organization)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.Code)
              .HasMaxLength(20);

            builder.Property(u => u.Title)
                .IsRequired();

            builder.Property(u => u.Author)
               .IsRequired();

            builder.Property(u => u.Url)
                .HasMaxLength(2000);

            builder.Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Books)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.Note)
                .HasMaxLength(9000);
        }
    }
}
