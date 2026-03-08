using Chik.Exams.Data;

namespace Chik.Exams;

public interface IServerErrorService
{
    IServerErrorRepository Repository { get; }
    Task<ServerError> Create(ServerError.Create dto);
    Task<ServerError> Get(Auth auth, Guid id);
    Task<List<ServerError>> Get(Auth auth, ServerError.Filter filter);
    Task<Paginated<ServerError>> Search(Auth auth, ServerError.Filter? filter = null, PaginationOptions? pagination = null);
}