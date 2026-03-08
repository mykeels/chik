using Chik.Exams.Logins.Repositories;

namespace Chik.Exams;

public interface ILoginService
{
    public ILoginRepository Repository { get; }
    Task Create(Auth auth, Login.Create login);
    Task<Paginated<Login>> Search(Auth auth, Login.Filter? filter = null, PaginationOptions? pagination = null);
}