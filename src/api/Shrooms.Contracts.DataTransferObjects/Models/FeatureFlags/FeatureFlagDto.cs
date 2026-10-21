using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags
{
    public class FeatureFlagDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public IEnumerable<FeatureFlagUserDto> Users { get; set; }
    }
}
