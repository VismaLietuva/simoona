using CommonMark;

namespace Shrooms.Domain.Helpers
{
    public class CommonMarkMarkdownConverter : IMarkdownConverter
    {
        private readonly CommonMarkSettings _settings;

        public CommonMarkMarkdownConverter()
            => _settings = BuildSettings();

        private static CommonMarkSettings BuildSettings()
        {
            var settings = CommonMarkSettings.Default;
            settings.AdditionalFeatures |= CommonMarkAdditionalFeatures.StrikethroughTilde;
            settings.RenderSoftLineBreaksAsLineBreaks = true;
            return settings;
        }

        public string ConvertToHtml(string markdown) => CommonMarkConverter.Convert(markdown, _settings);
    }
}
