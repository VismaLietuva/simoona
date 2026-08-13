namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosPieChartSliceViewModel
    {
        /// <summary>
        /// Display name, translated to the user's culture for built-in types.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stored, untranslated type name — the value to pass back as the
        /// kudos log's filteringType.
        /// </summary>
        public string TypeName { get; set; }

        public decimal Value { get; set; }
    }
}