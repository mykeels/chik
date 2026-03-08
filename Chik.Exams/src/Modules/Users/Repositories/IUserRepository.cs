namespace Chik.Exams.Data;

public interface IUserRepository
{
    Task<UserDbo> Create(User.Create user);
    Task<UserDbo?> Get(long id);
    Task<UserDbo?> Get(string username);
    Task<UserDbo> Update(long id, User.Update user);
    Task<Paginated<UserDbo>> Search(User.Filter? filter = null, PaginationOptions? pagination = null);
    Task Delete(long id);
}