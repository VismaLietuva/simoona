namespace Shrooms.Infrastructure.FeatureToggle
{
    public class AlwaysEnabledFeatureConfiguration : IFeatureConfiguration
    {
        public bool IsAvailable(Features feature) => true;
    }
}
