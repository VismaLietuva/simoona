using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Committee;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class CommitteeEntityConfig : IEntityTypeConfiguration<Committee>
    {
        public void Configure(EntityTypeBuilder<Committee> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasMany(a => a.Suggestions)
                .WithMany()
                .UsingEntity(j => j.ToTable("CommitteeSuggestionsIDs"));

            builder.HasOne(c => c.Organization)
                .WithMany()
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
