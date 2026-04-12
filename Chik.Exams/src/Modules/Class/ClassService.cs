using Chik.Exams.Data;

namespace Chik.Exams;

internal class ClassService(
    IClassRepository repository,
    ILogger<ClassService> logger
) : IClassService
{
    public IClassRepository Repository => repository;

    public async Task<Class> Get(Auth auth, int id)
    {
        logger.LogInformation($"{nameof(ClassService)}.{nameof(Get)} ({auth.Id}, {id})");
        var dbo = await repository.Get(id);
        if (dbo is null)
            throw new KeyNotFoundException($"Class with id '{id}' was not found");
        if (!auth.IsAdmin() && auth.IsTeacher())
        {
            var teacherClasses = await repository.GetClassIdsForTeacher(auth.Id);
            if (!teacherClasses.Contains(id))
                throw new UnauthorizedAccessException("You can only view classes you are assigned to");
        }
        return dbo.ToModel();
    }

    public async Task<List<Class>> List(Auth auth)
    {
        logger.LogInformation($"{nameof(ClassService)}.{nameof(List)} ({auth.Id})");
        if (!auth.IsAdmin() && !auth.IsTeacher())
            throw new UnauthorizedAccessException("Only Admin or Teacher can list classes");
        var items = await repository.List();
        if (auth.IsAdmin())
            return items.ConvertAll(c => c.ToModel());
        var teacherClasses = await repository.GetClassIdsForTeacher(auth.Id);
        return items.Where(c => teacherClasses.Contains(c.Id)).Select(c => c.ToModel()).ToList();
    }

    public async Task<Class> Create(Auth auth, Class.Create create)
    {
        logger.LogInformation($"{nameof(ClassService)}.{nameof(Create)} ({auth.Id})");
        if (!auth.IsAdmin())
            throw new UnauthorizedAccessException("Only Admin can create classes");
        var dbo = await repository.Create(create);
        return dbo.ToModel();
    }
}
