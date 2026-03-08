using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chik.Exams;

/// <summary>
/// We tried JsonStringEnumConverter, and it is great for new projects. However, older projects
/// are already depended on by other projects and have a lot of enums. Changing all of them to strings is not feasible.
///
/// This schema filter is a workaround for the issue. It adds the enum values to the schema description, which can
/// be used to generate the enum values in the client. This way, the API response can continue to be numeric enums, while
/// we will have the enum mappings with their string values in the generated client.
///
/// <example>
/// <code>
/// services.AddSwaggerGen(c =>
/// {
///    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
///    c.SchemaFilter&lt;EnumDescriptionSchemaFilter&gt;();
/// });
/// </code>
/// </example>
/// </summary>
public class EnumDescriptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumStringNames = Enum.GetNames(context.Type);
            IEnumerable<long> enumStringValues;
            try
            {
                enumStringValues = Enum.GetValues(context.Type).OfType<long>();
            }
            catch
            {
                enumStringValues = Enum.GetValues(context.Type)
                    .OfType<int>()
                    .Select(i => Convert.ToInt64(i));
            }
            var enumStringKeyValuePairs = enumStringNames.Zip(
                enumStringValues,
                (name, value) => $"{value} = {name}"
            );
            var enumStringNamesAsOpenApiArray = new OpenApiArray();
            enumStringNamesAsOpenApiArray.AddRange(
                enumStringNames.Select(name => new OpenApiString(name)).ToArray()
            );
            schema.Description = string.Join("\n", enumStringKeyValuePairs);
            schema.Extensions.Add("x-enum-varnames", enumStringNamesAsOpenApiArray);
        }
    }
}