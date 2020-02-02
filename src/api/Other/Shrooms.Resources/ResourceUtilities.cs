using System.Collections;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace Shrooms.Resources
{
    public class ResourceUtilities
    {
        public static object GetResource(string resource, string language)
        {
            var resourceManager = new ResourceManager($"Shrooms.Resources.{resource}", typeof(ResourceUtilities).Assembly);

            var culture = new CultureInfo(language);
            var resourceSet = resourceManager.GetResourceSet(culture, true, true);
            return resourceSet.Cast<DictionaryEntry>()
                              .ToDictionary(r => r.Key.ToString(), r => r.Value.ToString());
        }

        public static string GetResourceValue(string resource, string name, CultureInfo culture)
        {
            var resourceManager = new ResourceManager($"Shrooms.Resources.{resource}", typeof(ResourceUtilities).Assembly);
            return resourceManager.GetString(name, culture);
        }

        public static string GetResourceValue(ResourceManager resourceManager, string name, CultureInfo culture)
        {
            return resourceManager.GetString(name, culture);
        }
    }
}
