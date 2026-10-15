using Mapster;

namespace Shrooms.Presentation.ModelMappings.MapsterSetup
{
    /// <summary>
    /// Global Mapster defaults chosen to reproduce what AutoMapper does today, so swapping the mapper changes no API response.
    /// Each setting is pinned by MapsterSettingsTests.
    /// </summary>
    public static class MapsterSettings
    {
        private static readonly object GlobalSettingsLock = new();
        private static bool _globalSettingsApplied;

        /// <summary>
        /// A config with these defaults. Also applies them, once, to <see cref="TypeAdapterConfig.GlobalSettings"/>, which the
        /// <c>Adapt()</c> extension methods use: a stray <c>dto.Adapt&lt;T&gt;()</c> then behaves like an injected IMapper.
        /// </summary>
        public static TypeAdapterConfig Create()
        {
            ApplyToGlobalSettings();

            var config = new TypeAdapterConfig();
            Apply(config);
            return config;
        }

        public static void ApplyToGlobalSettings()
        {
            lock (GlobalSettingsLock)
            {
                if (_globalSettingsApplied)
                {
                    return;
                }

                Apply(TypeAdapterConfig.GlobalSettings);
                _globalSettingsApplied = true;
            }
        }

        public static void Apply(TypeAdapterConfig config)
        {
            config.Default
                // AutoMapper matches member names case-insensitively ("UserId" fills "UserID"); Mapster's default is exact.
                .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
                // AutoMapper maps a null source collection to an empty one; Mapster would return null, turning [] into null in JSON.
                .AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull)
                // AutoMapper assigns same-type members by reference; Mapster would deep-copy them, walking EF navigation graphs.
                .ShallowCopyForSameType(true)
                // Inlined nested mappings bypass PreserveReference: without this, Room -> Floor -> Rooms -> Room is not
                // recognised as a cycle and Mapster copies it until MaxDepth (or the stack) runs out.
                .AvoidInlineMapping(true);

            // Not global: PreserveReference. AutoMapper turns it on by itself for self-referencing types, so the
            // translated maps for those types set it explicitly. A global switch would cost every other mapping.
        }
    }
}
