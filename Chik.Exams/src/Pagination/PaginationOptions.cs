using System.Text.Json.Serialization;

namespace Chik.Exams;

public record PaginationOptions
{
    private int _page = DefaultPage;
    private int _pageSize = DefaultPageSize;
    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? DefaultPage : value;
    }
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? DefaultPageSize : value;
    }

    [JsonIgnore]
    public int Skip => (Page - 1) * PageSize;

    [JsonIgnore]
    public int Rows => PageSize;

    public PaginationOptions() { }

    public PaginationOptions(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 25;

    public PaginationOptions GetPaginationOptions()
    {
        return new PaginationOptions(Page, PageSize);
    }

    public string ToQueryString() => $"page={Page}&pageSize={PageSize}";
}