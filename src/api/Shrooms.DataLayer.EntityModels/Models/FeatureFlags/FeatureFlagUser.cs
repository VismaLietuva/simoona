namespace Shrooms.DataLayer.EntityModels.Models.FeatureFlags
{
    public class FeatureFlagUser : BaseModel
    {
        public int FeatureFlagId { get; set; }

        public virtual FeatureFlag FeatureFlag { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
