using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using AutoMapper.Internal;

namespace Chik.Exams;

/// <summary>
/// Swashbuckle has a problem with marking Enum properties as nullable in the generated schema.
///
/// This is a schema filter that fixes this issue by marking $ref enum properties in the schema as nullable.
///
/// <example>
/// <code>
/// services.AddSwaggerGen(c =>
/// {
///    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
///    c.SchemaFilter&lt;NullableEnumSchemaFilter&gt;();
/// });
/// </code>
/// </example>
///
/// <see href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2378" />
/// </summary>
public class NullableEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var isReferenceType =
            TypeHelper.IsReference(context.Type)
            && !TypeHelper.IsCLR(context.Type)
            && !TypeHelper.IsMicrosoft(context.Type);
        if (!isReferenceType)
        {
            return;
        }

        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        var members = context
            .Type.GetFields(bindingFlags)
            .Cast<MemberInfo>()
            .Concat(context.Type.GetProperties(bindingFlags))
            .ToArray();
        var hasNullableEnumMembers = members.Any(x => TypeHelper.IsNullableEnum(x.GetMemberType()));
        if (!hasNullableEnumMembers)
        {
            return;
        }

        schema
            .Properties.Where(x => !x.Value.Nullable)
            .ToList()
            .ForEach(property =>
            {
                var name = property.Key;
                var possibleNames = new string[] { name, TextCaseHelper.ToPascalCase(name) }; // handle different cases
                var sourceMember = possibleNames
                    .Select(n => context.Type.GetMember(n, bindingFlags).FirstOrDefault())
                    .Where(x => x != null)
                    .FirstOrDefault();
                if (sourceMember == null)
                {
                    return;
                }

                var sourceMemberType = sourceMember.GetMemberType();
                if (sourceMemberType == null || !TypeHelper.IsNullableEnum(sourceMemberType))
                {
                    return;
                }

                // manual nullability fixes
                if (property.Value.Reference != null)
                {
                    // option 1 - OpenAPI 3.1
                    // https://stackoverflow.com/a/48114924/5168794
                    //property.Value.AnyOf = new List<OpenApiSchema>()
                    //{
                    //    new OpenApiSchema
                    //    {
                    //        Type = "null",
                    //    },
                    //    new OpenApiSchema
                    //    {
                    //        Reference = property.Value.Reference,
                    //    },
                    //};
                    // property.Value.Reference = null;

                    // option 2 - OpenAPI 3.0
                    // https://stackoverflow.com/a/48114924/5168794
                    property.Value.Nullable = true;
                    property.Value.AllOf = new List<OpenApiSchema>()
                    {
                        new OpenApiSchema { Reference = property.Value.Reference },
                    };
                    property.Value.Reference = null;

                    // option 3 - OpenAPI 3.0
                    // https://stackoverflow.com/a/23737104/5168794
                    //property.Value.OneOf = new List<OpenApiSchema>()
                    //{
                    //    new OpenApiSchema
                    //    {
                    //        Type = "null",
                    //    },
                    //    new OpenApiSchema
                    //    {
                    //        Reference = property.Value.Reference,
                    //    },
                    //};
                    //property.Value.Reference = null;
                }
            });
    }
}

internal static class TextCaseHelper
{
    public static string ToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return char.ToUpper(text[0]) + text.Substring(1);
    }
}

internal static class TypeHelper
{
    /// <summary>
    /// Checks if type is CLR type.
    /// </summary>
    public static bool IsCLR(Type type) => type.Assembly == typeof(int).Assembly;

    /// <summary>
    /// Checks if type is Microsoft type.
    /// </summary>
    public static bool IsMicrosoft(Type type) =>
        type.Assembly.FullName?.StartsWith("Microsoft") ?? false;

    /// <summary>
    /// Checks if type is value type.
    /// </summary>
    public static bool IsValue(Type type) => type.IsValueType;

    /// <summary>
    /// Checks if type is reference type.
    /// </summary>
    public static bool IsReference(Type type) => !type.IsValueType && type.IsClass;

#if NETCOREAPP
    /// <summary>
    /// Checks if property type is nullable reference type.
    /// NB: Reflection APIs for nullability information are available from .NET 6 Preview 7.
    /// </summary>
    public static bool IsNullableReferenceProperty(PropertyInfo property) =>
        new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable;
#endif

    /// <summary>
    /// Checks if type is enum type.
    /// </summary>
    public static bool IsEnum(Type type) =>
        type.IsEnum || (Nullable.GetUnderlyingType(type)?.IsEnum ?? false);

    /// <summary>
    /// Checks if type is nullable enum type.
    /// </summary>
    public static bool IsNullableEnum(Type type) =>
        Nullable.GetUnderlyingType(type)?.IsEnum ?? false;

    /// <summary>
    /// Checks if type is not nullable enum type.
    /// </summary>
    public static bool IsNotNullableEnum(Type type) => IsEnum(type) && !IsNullableEnum(type);
}