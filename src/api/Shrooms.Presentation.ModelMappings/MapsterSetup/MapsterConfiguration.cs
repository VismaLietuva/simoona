using System;
using System.Reflection;
using Mapster;

namespace Shrooms.Presentation.ModelMappings.MapsterSetup
{
    /// <summary>
    /// The one Mapster configuration: <see cref="MapsterSettings"/> defaults plus every <see cref="IRegister"/> translation
    /// in the assemblies AutoMapper scans for profiles (IocBootstrapper). The parity tests build it exactly as the
    /// application will once it switches over.
    /// </summary>
    public static class MapsterConfiguration
    {
        public static readonly string[] AssemblyNames = { "Shrooms.Presentation.ModelMappings", "Shrooms.Premium" };

        public static TypeAdapterConfig Create()
        {
            var config = MapsterSettings.Create();
            config.Scan(Array.ConvertAll(AssemblyNames, Assembly.Load));
            return config;
        }
    }
}
