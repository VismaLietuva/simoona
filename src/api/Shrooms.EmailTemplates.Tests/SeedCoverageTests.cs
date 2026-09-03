using System.Reflection;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.EmailTemplates.Seeds;
using Shrooms.Premium.Constants;

namespace Shrooms.EmailTemplates.Tests
{
    // The golden files only cover what is seeded, so a template with no seed silently
    // escapes review. This is the test that fails when a new cache key skips the seeds.
    [TestFixture]
    public class SeedCoverageTests
    {
        private static IEnumerable<TestCaseData> CacheKeys()
        {
            return Keys(typeof(EmailTemplateCacheKeys))
                .Concat(Keys(typeof(EmailPremiumTemplateCacheKeys)))
                .Select(key => new TestCaseData(key).SetName(key));
        }

        [TestCaseSource(nameof(CacheKeys))]
        public void EveryCacheKey_HasASeed(string key)
        {
            Assert.That(
                EmailTemplateSeeds.All.Select(seed => seed.Key),
                Does.Contain(key),
                $"Add a seed for {key} to EmailTemplateSeeds, then run UPDATE_EMAIL_GOLDEN=1 dotnet test.");
        }

        private static IEnumerable<string> Keys(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                .Select(field => (string)field.GetRawConstantValue());
        }
    }
}
