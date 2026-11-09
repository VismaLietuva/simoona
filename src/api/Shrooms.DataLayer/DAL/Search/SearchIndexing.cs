namespace Shrooms.DataLayer.DAL.Search
{
    public static class SearchIndexing
    {
        public const string FeatureFlagName = "GlobalSearch";

        /// <summary>
        /// Kill switch for writing search rows on save (config "GlobalSearch:IndexOnSave").
        /// Off, saves never touch SearchDocuments, so the table can be dropped safely.
        /// </summary>
        public static bool IsEnabled { get; set; } = true;
    }
}
