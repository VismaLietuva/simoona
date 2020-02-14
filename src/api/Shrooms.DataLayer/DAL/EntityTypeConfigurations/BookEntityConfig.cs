using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Books;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class BookEntityConfig : EntityTypeConfiguration<Book>
    {
        public BookEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(x => x.Organization)
               .WithMany()
               .WillCascadeOnDelete(false);

            Property(u => u.Code)
              .HasMaxLength(20);

            Property(u => u.Title)
                .IsRequired();

            Property(u => u.Author)
               .IsRequired();

            Property(u => u.Url)
                .HasMaxLength(2000);

            Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            Property(u => u.CreatedBy)
                .HasMaxLength(50);

            HasOptional(x => x.ApplicationUser)
                .WithMany(x => x.Books)
                .WillCascadeOnDelete(false);

            Property(u => u.Note)
                .HasMaxLength(9000);
        }
    }
}
