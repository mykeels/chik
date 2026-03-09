using Chik.Exams.Data;
using Chik.Exams.Logins.Repositories;

namespace Chik.Exams;

public class LoginService(
    IJwtService jwtService,
    TimeProvider timeProvider,
    ILoginRepository repository,
    IUserRepository userRepository,
    ILogger<LoginService> logger
) : ILoginService
{
    public ILoginRepository Repository => repository;

    public async Task<User> Authenticate(string username, string password, string? ipAddress = null, string? userAgent = null)
    {
        logger.LogInformation($"{nameof(LoginService)}.{nameof(Authenticate)} ({username})");
        
        var userDbo = await userRepository.Get(username);
        if (userDbo is null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, userDbo.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Note: Login recording should be done by the caller after obtaining IpAddressLocation
        return userDbo!.ToModel();
    }

    public (string accessToken, string refreshToken) GenerateTokens(User user)
    {
        var accessToken = jwtService.GenerateToken(new Dictionary<string, string>
        {
            { "sub", user.Id.ToString() },
            { "username", user.Username },
            { "roles", string.Join(",", user.Roles.Select(role => role.ToString())) }
        });
        var refreshToken = jwtService.GenerateToken(new Dictionary<string, string>
        {
            { "sub", user.Id.ToString() }
        }, timeProvider.GetUtcNow().Add(TimeSpan.FromDays(1)).DateTime);
        return (accessToken, refreshToken);
    }

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