using Mapster;

namespace Shrooms.Presentation.ModelMappings.MapsterSetup
{
    /// <summary>
    /// Global Mapster defaults chosen to reproduce what AutoMapper does today, so swapping the mapper changes no API response.
    /// Each setting is pinned by MapsterSettingsTests.
    /// </summary>
    public static class MapsterSettings
    {
        public static TypeAdapterConfig Create()
        {
            var config = new TypeAdapterConfig();
            Apply(config);
            return config;
        }

        public static void Apply(TypeAdapterConfig config)
        {
            config.Default
                // AutoMapper matches member names case-insensitively ("UserId" fills "UserID"); Mapster's default is exact.
                .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
                // AutoMapper maps a null source collection to an empty one; Mapster would return null, turning [] into null in JSON.
                .AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull)
                // AutoMapper assigns same-type members by reference; Mapster would deep-copy them, walking EF navigation graphs.
                .ShallowCopyForSameType(true);

            // Not global: PreserveReference. AutoMapper turns it on by itself for self-referencing types, so the
            // translated maps for those types set it explicitly. A global switch would cost every other mapping.
        }
    }
}
