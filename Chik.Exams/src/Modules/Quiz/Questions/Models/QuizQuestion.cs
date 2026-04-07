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

            // NOTE: Do NOT use jsonObject.ToObject<ConcreteType>(serializer) here.
            // Since all concrete types inherit from QuestionType which has [JsonConverter],
            // passing the serializer causes infinite recursion. Instead, manually construct
            // each concrete type by reading the known fields directly from the JObject.
            QuestionType? obj = type switch
            {
                "single-choice" => new SingleChoice(jsonObject["options"].ToObject<List<Option>>(serializer)),
                "multiple-choice" => new MultipleChoice(jsonObject["options"].ToObject<List<Option>>(serializer)),
                "fill-in-the-blank" => new FillInTheBlank(jsonObject["acceptedAnswers"].ToObject<List<string>>(serializer)),
                "essay" => new Essay(jsonObject["minWords"].ToObject<int?>(serializer), jsonObject["maxWords"].ToObject<int?>(serializer)),
                "short-answer" => new ShortAnswer(jsonObject["acceptedAnswers"].ToObject<List<string>>(serializer), jsonObject["maxLength"].ToObject<int?>(serializer)),
                "true-or-false" => new TrueOrFalse(jsonObject["correctAnswer"].ToObject<bool>(serializer)),
                _ => throw new Newtonsoft.Json.JsonSerializationException($"Unknown question type: {type}")
            };

            return obj;
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            QuestionType? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is null) { writer.WriteNull(); return; }

            // Cannot call serializer.Serialize(writer, value, value.GetType()) here — all concrete
            // subtypes inherit [JsonConverter] from QuestionType, so Newtonsoft.Json would invoke
            // this converter again and detect a self-referencing loop.
            // Instead, build the JObject manually for each concrete type.
            var jo = new Newtonsoft.Json.Linq.JObject { ["type"] = value.Type };
            switch (value)
            {
                case SingleChoice sc:
                    jo["options"] = Newtonsoft.Json.Linq.JArray.FromObject(sc.Options, serializer);
                    break;
                case MultipleChoice mc:
                    jo["options"] = Newtonsoft.Json.Linq.JArray.FromObject(mc.Options, serializer);
                    break;
                case FillInTheBlank fitb:
                    jo["acceptedAnswers"] = Newtonsoft.Json.Linq.JArray.FromObject(fitb.AcceptedAnswers, serializer);
                    break;
                case Essay e:
                    jo["minWords"] = e.MinWords.HasValue ? (Newtonsoft.Json.Linq.JToken)e.MinWords.Value : Newtonsoft.Json.Linq.JValue.CreateNull();
                    jo["maxWords"] = e.MaxWords.HasValue ? (Newtonsoft.Json.Linq.JToken)e.MaxWords.Value : Newtonsoft.Json.Linq.JValue.CreateNull();
                    break;
                case ShortAnswer sa:
                    jo["acceptedAnswers"] = Newtonsoft.Json.Linq.JArray.FromObject(sa.AcceptedAnswers ?? [], serializer);
                    jo["maxLength"] = sa.MaxLength.HasValue ? (Newtonsoft.Json.Linq.JToken)sa.MaxLength.Value : Newtonsoft.Json.Linq.JValue.CreateNull();
                    break;
                case TrueOrFalse tof:
                    jo["correctAnswer"] = tof.CorrectAnswer;
                    break;
            }
            jo.WriteTo(writer);
        }
    }
}
