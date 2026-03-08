using System.Reflection;
#if NETCOREAPP
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif

namespace Chik.Exams;

#if NETCOREAPP
public class NullableReferenceSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // If any of the schema properties is a reference type
        // and it is nullable, then mark it as nullable
        foreach (var property in schema.Properties)
        {
            var propertyInfo = context.Type.GetProperty(
                property.Key,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
            );
            if (propertyInfo == null)
            {
                continue;
            }

            if (
                !propertyInfo.PropertyType.IsValueType
                && propertyInfo.PropertyType.IsReferenceOrNullableType()
                && property.Value.Reference != null
            )
            {
                schema.Required.Remove(property.Key);
                var reference = property.Value.Reference;
                property.Value.Reference = null;
                property.Value.AllOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Reference = reference },
                };
                property.Value.Nullable = true;
            }
        }
    }
}
#endif