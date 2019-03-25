using System.Collections;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;

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
                                    .ToDictionary(r => r.Key.ToString(),
                                                  r => r.Value.ToString());
        }

        public static string GetResourceValue(string resource, string name)
        {
            var resourceManager = new ResourceManager($"Shrooms.Resources.{resource}", typeof(ResourceUtilities).Assembly);
            return resourceManager.GetString(name, Thread.CurrentThread.CurrentCulture);
        }
    }
}
