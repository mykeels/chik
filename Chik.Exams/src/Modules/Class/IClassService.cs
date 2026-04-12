using Chik.Exams.Data;

namespace Chik.Exams;

public interface IClassService
{
    IClassRepository Repository { get; }

    Task<Class> Get(Auth auth, int id);

    Task<List<Class>> List(Auth auth);

    Task<Class> Create(Auth auth, Class.Create create);
}