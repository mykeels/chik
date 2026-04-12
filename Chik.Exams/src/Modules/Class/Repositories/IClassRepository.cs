namespace Chik.Exams.Data;

public interface IClassRepository
{
    Task<ClassDbo?> Get(int id);
    Task<List<ClassDbo>> List();
    Task<ClassDbo> Create(Class.Create create);
    Task<List<int>> GetClassIdsForTeacher(long userId);
}
