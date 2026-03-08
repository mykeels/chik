namespace Chik.Exams.Mails;

public class HelloWorldMailTemplate : EmailTemplate<object>
{
    public override Dictionary<string, object> SampleData => new()
    {
        { "Hello World", new object() }
    };

    public override string GetSubject()
    {
        return "Hello World";
    }

    public override Task<string> GetContentHtml()
    {
        string html = $@"
          <h1 class=""text-2xl font-bold"">Hello World</h1>
          <p>This is a test email.</p>
        ";
        string inlineHtml = InlineCss(html);
        return Task.FromResult(inlineHtml);
    }
}