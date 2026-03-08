namespace Chik.Exams.Data;

public interface IServerErrorRepository
{
    Task<ServerErrorDbo> Create(ServerErrorDbo serverError);
    Task<ServerErrorDbo?> Get(Guid id);
    Task<List<ServerErrorDbo>> Get(ServerError.Filter filter);
    Task<Paginated<ServerErrorDbo>> Search(ServerError.Filter? filter = null, PaginationOptions? pagination = null);
}