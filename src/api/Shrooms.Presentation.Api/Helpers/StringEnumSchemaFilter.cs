using System;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shrooms.Presentation.Api.Helpers
{
    public class StringEnumSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
            if (!type.IsEnum || schema is not OpenApiSchema concrete)
            {
                return;
            }

            concrete.Type = JsonSchemaType.String;
            concrete.Format = null;
            concrete.Enum = Enum.GetNames(type)
                .Select(name => (JsonNode)JsonValue.Create(name))
                .ToList();
        }
    }
}
