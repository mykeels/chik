using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.OpenApi.Any;

public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public void Transform(OpenApiSchema schema)
    {
        // This method is deprecated, use TransformAsync instead
    }

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        
        // Check if the type is an enum
        if (type.IsEnum)
        {
            // Check how the enum is being serialized (as string or as integer)
            // System.Text.Json typically serializes enums as strings by default
            var converter = context.JsonTypeInfo.Converter;
            var hasStringEnumConverter = converter is JsonStringEnumConverter || 
                                        context.JsonTypeInfo.Options.Converters.OfType<JsonStringEnumConverter>().Any();
            
            // Determine if we should use string or integer representation
            // Default to string unless explicitly configured otherwise
            var useStringEnum = hasStringEnumConverter || schema.Type == "string" || schema.Type == null;
            
            // Get all enum names
            var enumNames = Enum.GetNames(type);
            
            // Create the enum array for OpenAPI
            var enumArray = new List<IOpenApiAny>();
            
            if (useStringEnum)
            {
                // Serialize as strings (most common case)
                schema.Type = "string";
                
                foreach (var enumName in enumNames)
                {
                    enumArray.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
                }
            }
            else
            {
                // Serialize as integers
                schema.Type = "integer";
                var underlyingType = Enum.GetUnderlyingType(type);
                var enumValues = Enum.GetValues(type);
                
                foreach (var enumValue in enumValues)
                {
                    var numericValue = Convert.ChangeType(enumValue, underlyingType);
                    enumArray.Add(new Microsoft.OpenApi.Any.OpenApiInteger(Convert.ToInt32(numericValue)));
                }
            }
            
            schema.Enum = enumArray;
            
            // Add x-enum-varnames extension for code generation (matching the pattern in autogen script)
            var enumVarnames = new Microsoft.OpenApi.Any.OpenApiArray();
            foreach (var enumName in enumNames)
            {
                enumVarnames.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
            schema.Extensions["x-enum-varnames"] = enumVarnames;
            
            // Add descriptions if available (from Description attributes)
            var descriptions = new List<string>();
            foreach (var enumName in enumNames)
            {
                var field = type.GetField(enumName);
                var descriptionAttr = field?.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                descriptions.Add(descriptionAttr?.Description ?? enumName);
            }
            
            // Store descriptions in an extension for potential use by code generators
            if (descriptions.Any(d => !string.IsNullOrEmpty(d)))
            {
                var descriptionArray = new Microsoft.OpenApi.Any.OpenApiArray();
                foreach (var description in descriptions)
                {
                    descriptionArray.Add(new Microsoft.OpenApi.Any.OpenApiString(description));
                }
                schema.Extensions["x-enum-descriptions"] = descriptionArray;
            }
        }
        
        return Task.CompletedTask;
    }
}
