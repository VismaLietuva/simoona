using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags
{
    public class UpdateFeatureFlagDto
    {
        public bool IsEnabled { get; set; }

        public IEnumerable<string> UserIds { get; set; }
    }
}
