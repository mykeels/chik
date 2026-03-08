namespace Chik.Exams.Mails;

public interface IEmailTemplate
{
    string GetSubject();
    Task<string> GetContentHtml();
    Task<string> GetHtml();
}

public interface IEmailTemplate<TData>: IEmailTemplate, IEmailTemplateSampleDataRenderer where TData : class
{
    TData Data { get; }
    Dictionary<string, TData> SampleData { get; }
    IEmailTemplate<TData> WithData(TData data);
}

public interface IEmailTemplateSampleDataRenderer
{
    Task<List<string>> Render(string outputDirectory);
}