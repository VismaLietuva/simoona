using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Books;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class BookLogEntityConfig : EntityTypeConfiguration<BookLog>
    {
        public BookLogEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(u => u.BookOffice)
               .WithMany(x => x.BookLogs)
               .HasForeignKey(x => x.BookOfficeId)
               .WillCascadeOnDelete(false);

            HasRequired(x => x.Organization)
                .WithMany()
                .WillCascadeOnDelete(false);

            HasRequired(x => x.ApplicationUser)
                .WithMany(x => x.BookLogs)
                .WillCascadeOnDelete(false);

            Property(x => x.TakenFrom)
                .IsRequired();

            Property(u => u.ModifiedBy)
                .HasMaxLength(50);

            Property(u => u.CreatedBy)
                .HasMaxLength(50);

            Property(u => u.ApplicationUserId)
                .HasMaxLength(50);
        }
    }
}
