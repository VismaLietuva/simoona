using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeCategoryKudosTypeEntityConfiguration : IEntityTypeConfiguration<BadgeCategoryKudosType>
    {
        public void Configure(EntityTypeBuilder<BadgeCategoryKudosType> builder)
        {
            builder.HasOne(x => x.KudosType)
                .WithMany()
                .HasForeignKey(x => x.KudosTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasKey(type => type.Id);
        }
    }
}
