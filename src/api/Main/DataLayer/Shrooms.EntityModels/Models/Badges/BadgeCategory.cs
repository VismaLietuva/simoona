namespace Shrooms.EntityModels.Models.Badges
{
    public class BadgeCategory : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public BadgeCalculationPolicyType CalculationPolicyType { get; set; }
    }
}
