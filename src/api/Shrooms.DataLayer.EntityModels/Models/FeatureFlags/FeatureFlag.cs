using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models.FeatureFlags
{
    public class FeatureFlag : BaseModelWithOrg
    {
        public const int MaxNameLength = 100;

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public virtual ICollection<FeatureFlagUser> Users { get; set; }
    }
}
