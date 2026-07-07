using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shrooms.Presentation.Api.Helpers
{
    public class StringEnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
            if (!type.IsEnum)
            {
                return;
            }

            schema.Type = "string";
            schema.Format = null;
            schema.Enum = Enum.GetNames(type)
                .Select(name => (IOpenApiAny)new OpenApiString(name))
                .ToList();
        }
    }
}
