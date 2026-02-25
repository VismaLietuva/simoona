namespace Shrooms.Infrastructure.FeatureToggle
{
    public interface IFeatureConfiguration
    {
        bool IsAvailable(Features feature);
    }
}
