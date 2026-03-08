using Chik.Exams.Data;

namespace Chik.Exams.Logins.Repositories;

public interface ILoginRepository
{
    Task<LoginDbo> Create(long userId, Login.Create login);
    Task<LoginDbo?> GetLastLogin(long userId);
    Task<List<LoginDbo>> Get(Login.Filter filter);
    Task<Paginated<LoginDbo>> Search(Login.Filter? filter = null, PaginationOptions? pagination = null);
}