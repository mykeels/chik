namespace Chik.Exams;

public record QuizQuestion(
    long Id,
    long QuizId,
    string Prompt,
    int TypeId,
    QuizQuestion.QuestionType? Properties,
    int Score,
    int Order,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt
)
{
    public Quiz? Quiz { get; set; }
    public QuizQuestionType? Type { get; set; }

    public bool IsActive => DeactivatedAt is null;

    public record Create(
        long QuizId,
        string Prompt,
        long TypeId,
        string Properties,
        int Score,
        int Order
    );

    public record Update(
        long Id,
        string? Prompt = null,
        long? TypeId = null,
        string? Properties = null,
        int? Score = null,
        int? Order = null,
        DateTime? DeactivatedAt = null
    );

    public record Filter(
        long? QuizId = null,
        long? TypeId = null,
        bool? IsActive = null,
        DateTimeRange? DateRange = null,
        List<long>? QuestionIds = null,
        bool? IncludeQuiz = null,
        bool? IncludeType = null
    );

    /// <summary>
    /// Base record for question type properties.
    /// Serialized to JSON and stored in the Properties column.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(QuestionTypeJsonConverter))]
    public abstract record QuestionType(
        [property: Newtonsoft.Json.JsonProperty("type")]
        string Type
    );

    /// <summary>
    /// An option for choice-based questions.
    /// </summary>
    public record Option(
        [property: Newtonsoft.Json.JsonProperty("text")]
        string Text,
        [property: Newtonsoft.Json.JsonProperty("isCorrect")]
        bool IsCorrect
    );

    /// <summary>
    /// Single choice question - only one correct answer allowed.
    /// </summary>
    public record SingleChoice(
        [property: Newtonsoft.Json.JsonProperty("options")]
        List<Option> Options
    ) : QuestionType("single-choice");

    /// <summary>
    /// Multiple choice question - multiple correct answers allowed.
    /// </summary>
    public record MultipleChoice(
        [property: Newtonsoft.Json.JsonProperty("options")]
        List<Option> Options
    ) : QuestionType("multiple-choice");

    /// <summary>
    /// Fill in the blank question - user types in the answer.
    /// </summary>
    public record FillInTheBlank(
        [property: Newtonsoft.Json.JsonProperty("acceptedAnswers")]
        List<string> AcceptedAnswers
    ) : QuestionType("fill-in-the-blank");

    /// <summary>
    /// Essay question - free-form long text answer.
    /// </summary>
    public record Essay(
        [property: Newtonsoft.Json.JsonProperty("minWords")]
        int? MinWords = null,
        [property: Newtonsoft.Json.JsonProperty("maxWords")]
        int? MaxWords = null
    ) : QuestionType("essay");

    /// <summary>
    /// Short answer question - brief text answer.
    /// </summary>
    public record ShortAnswer(
        [property: Newtonsoft.Json.JsonProperty("acceptedAnswers")]
        List<string>? AcceptedAnswers = null,
        [property: Newtonsoft.Json.JsonProperty("maxLength")]
        int? MaxLength = null
    ) : QuestionType("short-answer");

    /// <summary>
    /// True or false question.
    /// </summary>
    public record TrueOrFalse(
        [property: Newtonsoft.Json.JsonProperty("correctAnswer")]
        bool CorrectAnswer
    ) : QuestionType("true-or-false");

    /// <summary>
    /// JSON converter for polymorphic QuestionType deserialization.
    /// </summary>
    public class QuestionTypeJsonConverter : Newtonsoft.Json.JsonConverter<QuestionType>
    {
        public override QuestionType? ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            QuestionType? existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
            var type = (string?)jsonObject["type"];

            return type switch
            {
                "single-choice" => jsonObject.ToObject<SingleChoice>(serializer),
                "multiple-choice" => jsonObject.ToObject<MultipleChoice>(serializer),
                "fill-in-the-blank" => jsonObject.ToObject<FillInTheBlank>(serializer),
                "essay" => jsonObject.ToObject<Essay>(serializer),
                "short-answer" => jsonObject.ToObject<ShortAnswer>(serializer),
                "true-or-false" => jsonObject.ToObject<TrueOrFalse>(serializer),
                _ => throw new Newtonsoft.Json.JsonSerializationException($"Unknown question type: {type}")
            };
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            QuestionType? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, value?.GetType());
        }
    }
}
