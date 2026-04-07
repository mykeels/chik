using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chik.Exams;

/// <summary>
/// Adds additionalProperties: {} to QuizQuestion.QuestionType so that generated clients
/// (e.g. openapi-zod-client) preserve fields like 'options' and 'correctAnswer' that belong
/// to concrete subtypes but are not declared on the abstract base schema.
/// </summary>
public class QuestionTypeSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(QuizQuestion.QuestionType))
        {
            schema.AdditionalPropertiesAllowed = true;
            schema.AdditionalProperties = new OpenApiSchema();
        }
    }
}
