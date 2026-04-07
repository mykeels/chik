using System.ComponentModel.DataAnnotations;
using Chik.Exams.Data;

namespace Chik.Exams;

/// <summary>
/// Bidirectional mapping between QuizYamlDocument/QuizYamlQuestion and
/// the internal QuizQuestion.QuestionType JSON format stored in the DB.
/// </summary>
public static class QuizPortMapper
{
    /// <summary>
    /// Converts an internal QuizQuestion model to a YAML-schema-compatible QuizYamlQuestion.
    /// Called once per question during export.
    /// </summary>
    public static QuizYamlQuestion ToYaml(QuizQuestion question)
    {
        var props = question.Properties switch
        {
            QuizQuestion.MultipleChoice mc => new Dictionary<string, object>
            {
                ["options"] = mc.Options.Select(o => (object)new Dictionary<string, object>
                {
                    ["text"] = o.Text,
                    ["isCorrect"] = o.IsCorrect
                }).ToList()
            },
            QuizQuestion.SingleChoice sc => new Dictionary<string, object>
            {
                ["options"] = sc.Options.Select(o => (object)new Dictionary<string, object>
                {
                    ["text"] = o.Text,
                    ["isCorrect"] = o.IsCorrect
                }).ToList()
            },
            // YAML schema uses "answer", internal DB uses "correctAnswer"
            QuizQuestion.TrueOrFalse tof => new Dictionary<string, object>
            {
                ["answer"] = (object)tof.CorrectAnswer
            },
            // fill-in-the-blank, essay, short-answer → empty properties block
            _ => new Dictionary<string, object>()
        };

        return new QuizYamlQuestion
        {
            Prompt = question.Prompt,
            Type = question.Properties?.Type ?? string.Empty,
            Score = question.Score,
            Properties = props
        };
    }

    /// <summary>
    /// Converts a YAML question to the internal JSON Properties string stored in the DB.
    /// Called once per question during import.
    /// </summary>
    public static string ToInternalProperties(QuizYamlQuestion yaml)
    {
        QuizQuestion.QuestionType properties = yaml.Type switch
        {
            "multiple-choice" => new QuizQuestion.MultipleChoice(ParseOptions(yaml)),
            "single-choice" => new QuizQuestion.SingleChoice(ParseOptions(yaml)),
            // YAML schema uses "answer", internal DB uses "correctAnswer"
            "true-or-false" => new QuizQuestion.TrueOrFalse(ParseAnswer(yaml)),
            "fill-in-the-blank" => new QuizQuestion.FillInTheBlank([]),
            "essay" => new QuizQuestion.Essay(null, null),
            "short-answer" => new QuizQuestion.ShortAnswer([], null),
            _ => throw new ValidationException($"Unknown question type: '{yaml.Type}'")
        };

        return QuizQuestionDbo.SerializeProperties(properties);
    }

    /// <summary>
    /// Maps a YAML question type string to the DB TypeId (matches seeded quiz_question_types).
    /// </summary>
    public static int ToTypeId(string yamlType) => yamlType switch
    {
        "multiple-choice" => 1,
        "single-choice" => 2,
        "fill-in-the-blank" => 3,
        "essay" => 4,
        "short-answer" => 5,
        "true-or-false" => 6,
        _ => throw new ValidationException($"Unknown question type: '{yamlType}'")
    };

    /// <summary>Converts a nullable TimeSpan to HH:MM:SS string, or null.</summary>
    public static string? ToDuration(TimeSpan? ts) =>
        ts is null ? null : ts.Value.ToString(@"hh\:mm\:ss");

    /// <summary>Parses a nullable HH:MM:SS duration string to TimeSpan.</summary>
    public static TimeSpan? FromDurationString(string? s)
    {
        if (s is null) return null;
        if (!TimeSpan.TryParseExact(s, @"hh\:mm\:ss", null, out var ts))
            throw new ValidationException("'duration' must be in HH:MM:SS format, e.g. '01:30:00'");
        return ts;
    }

    private static List<QuizQuestion.Option> ParseOptions(QuizYamlQuestion yaml)
    {
        if (yaml.Properties is null || !yaml.Properties.ContainsKey("options"))
            throw new ValidationException($"'properties.options' is required for type '{yaml.Type}'");

        // YamlDotNet deserializes nested mappings as Dictionary<object, object>
        var rawOptions = (List<object>)yaml.Properties["options"];
        return rawOptions.Select(o =>
        {
            var dict = (Dictionary<object, object>)o;
            return new QuizQuestion.Option(
                dict["text"].ToString()!,
                Convert.ToBoolean(dict["isCorrect"])
            );
        }).ToList();
    }

    private static bool ParseAnswer(QuizYamlQuestion yaml)
    {
        if (yaml.Properties is null || !yaml.Properties.ContainsKey("answer"))
            throw new ValidationException("'properties.answer' is required for type 'true-or-false'");
        return Convert.ToBoolean(yaml.Properties["answer"]);
    }
}
