namespace Shrooms.Presentation.Common.Filters
{
    /// <summary>
    /// Output cache tags are global to the process, so every tag carries its organization. Evicting
    /// one organization's widgets must never drop another tenant's entries.
    /// </summary>
    public static class WidgetCacheTag
    {
        public const string WallWidgets = "WallWidgets";
        public const string LotteryWidget = "LotteryWidget";

        public static string For(string cacheGroup, string tenant)
        {
            return $"{cacheGroup}:{tenant}";
        }
    }
}
