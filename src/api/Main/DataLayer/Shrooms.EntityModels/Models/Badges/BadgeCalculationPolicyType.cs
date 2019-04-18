namespace Shrooms.EntityModels.Models.Badges
{
    public enum BadgeCalculationPolicyType
    {
        PointsStrategy = 1, // SUM(multiplier * kudos count)
        MultiplierStrategy = 2 // SUM(multiplier)
    }
}