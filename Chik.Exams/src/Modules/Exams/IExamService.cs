using Chik.Exams.Data;

namespace Chik.Exams;

public interface IExamService
{
    public IExamRepository Repository { get; }

    Task<ExamDbo> Create(Auth auth, Exam.Create exam);
}