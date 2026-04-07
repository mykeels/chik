namespace Chik.Exams;

public interface IQuizImportExportService
{
    /// <summary>
    /// Builds a ZIP archive containing index.yaml and returns it as bytes with a suggested filename.
    /// Throws KeyNotFoundException if the quiz does not exist.
    /// Throws UnauthorizedAccessException if the caller cannot access the quiz.
    /// </summary>
    Task<(byte[] ZipBytes, string FileName)> ExportQuiz(Auth auth, long quizId);

    /// <summary>
    /// Parses a ZIP archive from a stream, validates the YAML, creates the quiz and all questions.
    /// Returns the created Quiz with Questions populated.
    /// Throws ValidationException with a descriptive message on any structural error.
    /// </summary>
    Task<Quiz> ImportQuiz(Auth auth, Stream zipStream, long? examinerId);
}
