using Chik.Exams.Data;

namespace Chik.Exams;

public class ServerErrorService(
    IServerErrorRepository repository,
    ILogger<ServerErrorService> logger,
    IEmailService emailService,
    RemoteEnvironment remoteEnvironment
) : IServerErrorService
{
    public IServerErrorRepository Repository => repository;

    public async Task<ServerError> Create(ServerError.Create dto)
    {
        var dbo = await Repository.Create(new ServerErrorDbo()
        {
            UserId = dto.UserId,
            OperationId = dto.OperationId,
            RequestPath = dto.RequestPath,
            RequestMethod = dto.RequestMethod,
            ErrorJson = dto.ErrorJson,
            Error = dto.Error,
            ErrorAt = dto.ErrorAt,
        });
        if (dbo is null)
        {
            throw new Exception("Failed to create client error");
        }
        var serverError = (ServerError)dbo!;
        var errorEndpoint = $"{serverError.RequestMethod} {remoteEnvironment.GetBaseUrl() + "/" + serverError.RequestPath?.TrimStart('/')}";
        await emailService.SendEmail(
            Auth.Admin.Email,
            "Server Error at " + errorEndpoint,
            MarkdownService.ToHtml(string.Join(
                "<br />",
                [
                    $"## Server Error at {errorEndpoint}",
                    "",
                    "- Timestamp: " + serverError.ErrorAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    "- User: " + serverError.UserId,
                    "- Operation: " + serverError.OperationId,
                    "- Request Path: " + serverError.RequestPath,
                    "- Request Method: " + serverError.RequestMethod,
                    "",
                    "```",
                    "Error: " + serverError.Error,
                    "```",
                    "",
                    "```json",
                    serverError.ErrorJson,
                    "```",
                ]
            ))
        );
        return serverError;
    }

    public async Task<ServerError> Get(Auth auth, Guid id)
    {
        var dbo = await Repository.Get(id);
        if (dbo is null)
        {
            throw new KeyNotFoundException($"Client error {id} not found");
        }
        var serverError = (ServerError)dbo!;
        serverError.User = auth;
        return serverError;
    }

    public async Task<List<ServerError>> Get(Auth auth, ServerError.Filter filter)
    {
        var dbo = await Repository.Get(filter);
        return dbo.Select(dbo => (ServerError)dbo!).ToList();
    }

    public async Task<Paginated<ServerError>> Search(Auth auth, ServerError.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new ServerError.Filter();
        if (!auth.IsAdmin())
        {
            filter = filter with { UserId = auth.Id };
        }
        var dbo = await Repository.Search(filter, pagination);
        return new Paginated<ServerError>(
            dbo.Items.Select(dbo => (ServerError)dbo!).ToList(),
            dbo.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }
}