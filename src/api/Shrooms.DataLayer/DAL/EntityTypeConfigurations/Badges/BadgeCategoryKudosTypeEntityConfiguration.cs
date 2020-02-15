using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges
{
    internal class BadgeCategoryKudosTypeEntityConfiguration : EntityTypeConfiguration<BadgeCategoryKudosType>
    {
        public BadgeCategoryKudosTypeEntityConfiguration()
        {
            HasRequired(x => x.BadgeCategory)
                .WithRequiredDependent()
                .WillCascadeOnDelete(false);

            HasRequired(x => x.KudosType)
                .WithRequiredDependent()
                .WillCascadeOnDelete(false);

            HasKey(type => type.Id);
        }
    }
}
