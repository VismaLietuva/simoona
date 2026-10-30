using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shrooms.Presentation.ModelMappings
{
    /// <summary>
    /// The assemblies that hold mapping configuration: AutoMapper profiles and the Mapster translations next to them.
    /// IocBootstrapper, MapsterConfiguration and the parity tests all read this list, so an assembly added here is covered
    /// by all three.
    /// </summary>
    public static class MappingAssemblies
    {
        public static IReadOnlyList<string> Names { get; } = Array.AsReadOnly(new[] { "Shrooms.Presentation.ModelMappings", "Shrooms.Premium" });

        public static Assembly[] Load()
        {
            return Names.Select(Assembly.Load).ToArray();
        }
    }
}
