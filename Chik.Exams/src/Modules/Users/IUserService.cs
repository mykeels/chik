using Chik.Exams.Data;

namespace Chik.Exams;

public interface IUserService
{
    public IUserRepository Repository { get; }

    Task<User> Create(Auth auth, User.Create user);
}