using System.ComponentModel.DataAnnotations;

namespace Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags
{
    public class NewFeatureFlagDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
