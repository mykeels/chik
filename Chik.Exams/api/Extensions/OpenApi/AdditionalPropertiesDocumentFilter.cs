using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chik.Exams;

/// <summary>
/// A document filter that sets the AdditionalPropertiesAllowed flag to true for schemas that have null AdditionalProperties.
/// </summary>
public class AdditionalPropertiesDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
    {
        foreach (var schema in context.SchemaRepository.Schemas)
        {
            if (schema.Value.AdditionalProperties == null)
            {
                schema.Value.AdditionalPropertiesAllowed = true;
            }
        }
    }
}