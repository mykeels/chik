using Chik.Exams.Logins.Repositories;

namespace Chik.Exams;

public class LoginService(
    ILoginRepository repository,
    ILogger<LoginService> logger
) : ILoginService
{
    public ILoginRepository Repository => repository;

    public async Task Create(Auth auth, Login.Create login)
    {
        logger.LogInformation($"{nameof(LoginService)}.{nameof(Create)} ({auth.Id}, {login})");
        await repository.Create(auth.Id, login);
    }

    public async Task<Paginated<Login>> Search(Auth auth, Login.Filter? filter = null, PaginationOptions? pagination = null)
    {
        filter ??= new Login.Filter();
        if (!auth.IsAdmin())
        {
            filter = filter with { UserId = auth.Id };
        }
        pagination ??= new PaginationOptions();
        var paginated = await repository.Search(filter, pagination);
        return new Paginated<Login>(
            paginated.Items.Select(dbo => (Login)dbo!).ToList(), 
            paginated.TotalCount, 
            pagination,
            async options => await Search(auth, filter, options)
        );
    }
}