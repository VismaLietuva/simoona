using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.EntityModels.Models.Badges
{
    public class BadgeCategoryKudosType
    {
        public int Id { get; set; }
        public BadgeCalculationPolicyType CalculationPolicyType { get; set; }

        public int BadgeCategoryId { get; set; }
        public int KudosTypeId { get; set; }

        public virtual BadgeCategory BadgeCategory { get; set; }
        public virtual KudosType KudosType { get; set; }
    }
}
