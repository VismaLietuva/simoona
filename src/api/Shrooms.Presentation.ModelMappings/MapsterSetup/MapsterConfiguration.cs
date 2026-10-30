using Mapster;

namespace Shrooms.Presentation.ModelMappings.MapsterSetup
{
    /// <summary>
    /// The one Mapster configuration: <see cref="MapsterSettings"/> defaults plus every <see cref="IRegister"/> translation
    /// in <see cref="MappingAssemblies"/>, where AutoMapper finds its profiles. The parity tests build it exactly as the
    /// application will once it switches over.
    /// </summary>
    public static class MapsterConfiguration
    {
        public static TypeAdapterConfig Create()
        {
            var config = MapsterSettings.Create();
            config.Scan(MappingAssemblies.Load());
            return config;
        }
    }
}
