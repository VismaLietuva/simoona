using System.Text.RegularExpressions;
using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.EmailTemplates.Seeds;

namespace Shrooms.EmailTemplates.Tests
{
    // Renders every template and compares it to the approved baseline, ignoring indentation.
    [TestFixture]
    public class GoldenFileTests
    {
        private static readonly string GoldenRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "GoldenFiles");

        // UPDATE_EMAIL_GOLDEN=1 rewrites the baseline instead of asserting, for intended changes.
        private static bool UpdateGolden => Environment.GetEnvironmentVariable("UPDATE_EMAIL_GOLDEN") == "1";

        private static string SourceGoldenRoot =>
            Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../GoldenFiles"));

        private static IEnumerable<TestCaseData> Seeds()
        {
            return EmailTemplateSeeds.All.Select(seed => new TestCaseData(seed).SetName(seed.Key));
        }

        [TestCaseSource(nameof(Seeds))]
        public async Task Template_MatchesGoldenFile(EmailTemplateSeed seed)
        {
            var rendered = await RazorTemplateEngine.RenderAsync(seed.Key, seed.Model);

            if (UpdateGolden)
            {
                Directory.CreateDirectory(SourceGoldenRoot);
                await File.WriteAllTextAsync(Path.Combine(SourceGoldenRoot, GoldenFileName(seed.Key)), rendered);
                Assert.Pass("Golden file rewritten.");
            }

            var goldenPath = Path.Combine(GoldenRoot, GoldenFileName(seed.Key));
            Assert.That(File.Exists(goldenPath), Is.True, $"Missing golden file for {seed.Key}. Expected {goldenPath}.");

            Assert.That(Normalize(rendered), Is.EqualTo(Normalize(await File.ReadAllTextAsync(goldenPath))));
        }

        [Test]
        public void EverySeededTemplate_HasAGoldenFile()
        {
            var expected = EmailTemplateSeeds.All.Select(seed => GoldenFileName(seed.Key)).OrderBy(name => name);
            var actual = Directory.GetFiles(GoldenRoot, "*.html").Select(Path.GetFileName).OrderBy(name => name);

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static string GoldenFileName(string key)
        {
            return key.TrimStart('/').Replace("EmailTemplates/", string.Empty).Replace(".cshtml", string.Empty).Replace('/', '.') + ".html";
        }

        private static string Normalize(string html)
        {
            // Collapses runs rather than deleting them: whitespace between inline elements
            // is rendered, so "</strong> <span>" must not compare equal to "</strong><span>".
            return Regex.Replace(html, @"\s+", " ").Trim();
        }
    }
}
