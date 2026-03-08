using Chik.Exams.Data;

namespace Chik.Exams;

internal class ExamService(
    IExamRepository repository,
    ILogger<ExamService> logger
) : IExamService
{
    public IExamRepository Repository => repository;

    public async Task<ExamDbo> Create(Auth auth, Exam.Create exam)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Create)} ({auth.Id}, {exam})");
        return await repository.Create(exam);
    }
}