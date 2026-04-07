namespace Chik.Exams;

/// <summary>
/// Root document matching quiz.schema.json. Used by YamlDotNet for serialization/deserialization.
/// CamelCaseNamingConvention maps PascalCase C# properties to camelCase YAML keys.
/// </summary>
public class QuizYamlDocument
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>HH:MM:SS string, or null for no time limit.</summary>
    public string? Duration { get; set; }
    public List<QuizYamlQuestion> Questions { get; set; } = [];
}

public class QuizYamlQuestion
{
    public string Prompt { get; set; } = string.Empty;
    /// <summary>One of: multiple-choice, single-choice, true-or-false, fill-in-the-blank, essay, short-answer</summary>
    public string Type { get; set; } = string.Empty;
    public int Score { get; set; }
    /// <summary>
    /// Kept as Dictionary so YamlDotNet can deserialize the polymorphic properties block
    /// without a custom converter. An empty YAML {} block deserializes to an empty dict (not null).
    /// Note: YamlDotNet gives nested mappings as Dictionary&lt;object, object&gt;, not Dictionary&lt;string, object&gt;.
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
}
