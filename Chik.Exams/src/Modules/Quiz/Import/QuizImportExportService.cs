using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using Chik.Exams.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Chik.Exams;

internal class QuizImportExportService(
    IQuizService quizService,
    IQuizQuestionRepository questionRepository,
    IFileStorage fileStorage,
    IAuditLogService auditLogService,
    ILogger<QuizImportExportService> logger
) : IQuizImportExportService
{
    public async Task<(byte[] ZipBytes, string FileName)> ExportQuiz(Auth auth, long quizId)
    {
        logger.LogInformation($"{nameof(QuizImportExportService)}.{nameof(ExportQuiz)} ({auth.Id}, {quizId})");

        var quiz = await quizService.Get(auth, quizId, includeQuestions: true);
        if (quiz is null)
            throw new KeyNotFoundException($"Quiz with id '{quizId}' not found");

        var document = new QuizYamlDocument
        {
            Title = quiz.Title,
            Description = quiz.Description,
            Duration = QuizPortMapper.ToDuration(quiz.Duration),
            Questions = quiz.Questions?.Select(QuizPortMapper.ToYaml).ToList() ?? []
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        string yaml = serializer.Serialize(document);

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        string tempFolder = $"quizzes/exports/{quizId}-{timestamp}";
        string relativeZipPath = $"{tempFolder}.zip";

        fileStorage.CreateDirectory(tempFolder);

        try
        {
            using var yamlStream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
            await fileStorage.SaveFileAsync($"{tempFolder}/index.yaml", yamlStream);

            await fileStorage.ZipFolderAsync(tempFolder);

            byte[] bytes;
            using (var zipStream = await fileStorage.ReadFileAsync(relativeZipPath))
            using (var ms = new MemoryStream())
            {
                await zipStream.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            await auditLogService.Create(auth, new AuditLog.Create<object>(
                $"{nameof(QuizImportExportService)}.{nameof(ExportQuiz)}",
                quizId,
                new { QuizId = quizId }
            ));

            var safeTitle = Regex.Replace(quiz.Title, @"[^\w\-]", "-").ToLowerInvariant();
            string fileName = $"quiz-{quizId}-{safeTitle}.zip";

            return (bytes, fileName);
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(tempFolder);
            await fileStorage.DeleteFileAsync(relativeZipPath);
        }
    }

    public async Task<Quiz> ImportQuiz(Auth auth, Stream zipStream, long? examinerId)
    {
        logger.LogInformation($"{nameof(QuizImportExportService)}.{nameof(ImportQuiz)} ({auth.Id})");

        if (!auth.IsAdmin() && !auth.IsTeacher())
            throw new UnauthorizedAccessException("Only Admin or Teacher can import quizzes");

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        string tempZipRelative = $"quizzes/imports/{timestamp}.zip";
        string extractedRelative = $"quizzes/imports/{timestamp}";

        await fileStorage.SaveFileAsync(tempZipRelative, zipStream);

        string extractedAbsolutePath;
        try
        {
            extractedAbsolutePath = await fileStorage.UnzipAsync(tempZipRelative);
        }
        catch (InvalidDataException)
        {
            await fileStorage.DeleteFileAsync(tempZipRelative);
            throw new ValidationException("The uploaded file is not a valid ZIP archive");
        }

        try
        {
            string yamlFilePath = Path.Combine(extractedAbsolutePath, "index.yaml");
            if (!File.Exists(yamlFilePath))
                throw new ValidationException("ZIP archive must contain 'index.yaml' at the root");

            string yamlText = await File.ReadAllTextAsync(yamlFilePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            QuizYamlDocument document;
            try
            {
                document = deserializer.Deserialize<QuizYamlDocument>(yamlText);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"index.yaml is malformed: {ex.Message}");
            }

            ValidateDocument(document);

            var duration = QuizPortMapper.FromDurationString(document.Duration);

            var quiz = await quizService.Create(auth, new Quiz.Create(
                document.Title,
                document.Description ?? string.Empty,
                auth.Id,
                examinerId,
                duration
            ));

            var createdQuestions = new List<QuizQuestion>();
            for (int i = 0; i < document.Questions.Count; i++)
            {
                var yamlQ = document.Questions[i];
                int typeId = QuizPortMapper.ToTypeId(yamlQ.Type);
                string propertiesJson = QuizPortMapper.ToInternalProperties(yamlQ);
                var questionDbo = await questionRepository.Create(new QuizQuestion.Create(
                    quiz.Id,
                    yamlQ.Prompt,
                    typeId,
                    propertiesJson,
                    yamlQ.Score,
                    Order: i + 1
                ));
                createdQuestions.Add(questionDbo.ToModel());
            }

            await auditLogService.Create(auth, new AuditLog.Create<object>(
                $"{nameof(QuizImportExportService)}.{nameof(ImportQuiz)}",
                quiz.Id,
                new { QuizId = quiz.Id, QuestionCount = createdQuestions.Count }
            ));

            quiz.Questions = createdQuestions;
            return quiz;
        }
        finally
        {
            await fileStorage.DeleteFileAsync(tempZipRelative);
            await fileStorage.DeleteFolderAsync(extractedRelative);
        }
    }

    private static void ValidateDocument(QuizYamlDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.Title))
            throw new ValidationException("'title' is required");

        if (document.Questions is null || document.Questions.Count == 0)
            throw new ValidationException("'questions' must contain at least one question");

        string[] validTypes = ["multiple-choice", "single-choice", "fill-in-the-blank", "essay", "short-answer", "true-or-false"];

        for (int i = 0; i < document.Questions.Count; i++)
        {
            var q = document.Questions[i];
            int n = i + 1;

            if (string.IsNullOrWhiteSpace(q.Prompt))
                throw new ValidationException($"Question {n}: 'prompt' is required");

            if (q.Score < 0)
                throw new ValidationException($"Question {n}: 'score' must be >= 0");

            if (!validTypes.Contains(q.Type))
                throw new ValidationException($"Question {n}: unknown type '{q.Type}'");

            if (q.Type is "multiple-choice" or "single-choice")
            {
                if (q.Properties is null || !q.Properties.ContainsKey("options"))
                    throw new ValidationException($"Question {n}: 'properties.options' is required for type '{q.Type}'");
            }

            if (q.Type == "true-or-false")
            {
                if (q.Properties is null || !q.Properties.ContainsKey("answer"))
                    throw new ValidationException($"Question {n}: 'properties.answer' is required for type 'true-or-false'");
            }
        }
    }
}
