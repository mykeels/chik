using System.Text.RegularExpressions;
using System.Web;

namespace Chik.Exams.Mails;

public abstract class EmailTemplate<TData> : IEmailTemplate<TData> where TData : class
{
    private TData? _data;
    public TData Data => _data!;
    public virtual Dictionary<string, TData> SampleData { get; } = new();

    public IEmailTemplate<TData> WithData(TData data)
    {
        _data = data;
        return this;
    }

    public abstract string GetSubject();
    public abstract Task<string> GetContentHtml();

    public string AuthenticatedUrl(string url)
    {
        var remoteEnvironment = Provider.GetRequiredService<RemoteEnvironment>();
        return remoteEnvironment.GetAuthenticatedUrl(url);
    }
    public string SupportEmail => "support@under4.games";
    public string SupportEmailUrl => $"mailto:{SupportEmail}";
    public string SupportEmailLink => Link(SupportEmailUrl, SupportEmail);
    public string BillingUrl
    {
        get
        {
            var remoteEnvironment = Provider.GetRequiredService<RemoteEnvironment>();
            var appUrl = remoteEnvironment.GetAppUrl();
            var billingUrl = $"{appUrl}/billing";
            return billingUrl;
        }
    }

    public async Task<List<string>> Render(string outputDirectory)
    {
        var fileNames = new List<string>();
        foreach (var sampleData in SampleData)
        {
            var instance = WithData(sampleData.Value);
            var html = await instance.GetHtml();
            var subject = instance.GetSubject();
            string fileName = $"{sampleData.Key}/index.html";
            string filePath = Path.Combine(outputDirectory, fileName);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(filePath, html);
            fileNames.Add(fileName);
        }
        return fileNames;
    }

    protected string HeaderHtml()
    {
        return @$"
            <header class=""w-full"">
                <img src=""https://res.cloudinary.com/mykeels/image/upload/c_fill,g_auto,e_sharpen/v1761930461/Under%204%20Games/Under_4_Games_Email_Banner_ydue0e.png"" alt=""Under 4 Games"" style=""width: 100%;"" />
            </header>
        ";
    }

    protected string FooterHtml()
    {
        return @$"
            <footer class=""py-2 justify-center w-full"">
                <table class=""w-full border-none mb-2"">
                    <tr>
                        <td class=""text-center"">
                            {Link("https://exams.chik.ng", ApplicationConstants.Name)}
                        </td>
                    </tr>
                </table>
                <div class=""text-sm mt-2 text-center"">© 2024 {ApplicationName()}. All rights reserved.</div>
            </footer>
        ";
    }

    protected string YoursSincerely()
    {
        return @$"
            <div class=""mt-2"">
                {P("Yours sincerely,")}
                {P($"Michael Ikechi - Creator of {ApplicationName()}")}
            </div>
        ";
    }

    protected string ApplicationName()
    {
        return ApplicationConstants.Name;
    }

    public async Task<string> GetHtml()
    {
        string html = @$"
             <div class=""bg-white rounded-lg text-gray-700 text-lg"" style=""min-width: 480px; width: 100%;"">
                {HeaderHtml()}
                <main class=""p-4"">
                    {await GetContentHtml()}
                    {YoursSincerely()}
                </main>
                {FooterHtml()}
            </div>
        ";
        return InlineCss(html);
    }

    protected string Link(string url, string text)
    {
        return @$"
            <a href=""{url}"" class=""text-blue-500 underline"" target=""_blank"">{text}</a>
        ";
    }

    protected string CTA(string url, string text, string backgroundColor = "#BB017A")
    {
        return @$"
            <a href=""{url}"" class=""inline-block text-white px-4 py-2 my-2 rounded transition-colors"" target=""_blank"" style=""background-color: {backgroundColor};"">
                {text}
            </a>
        ";
    }

    protected string Center(string html)
    {
        return @$"
            <center>{html}</center>
        ";
    }

    protected string Strong(string text)
    {
        return @$"
            <strong class=""font-bold scale-110"">{text}</strong>
        ";
    }

    protected string P(string text, Func<string, string>? className = null)
    {
        className ??= (classes) => classes ?? "";
        return @$"
            <p class=""{className("py-1 my-1 w-full")}"">{text}</p>
        ";
    }

    protected string Quote(string text)
    {
        return @$"
            <blockquote class=""py-2 my-4 border-l-4 border-gray-300 pl-4 text-xl italic bg-white text-gray-700"">{text}</blockquote>
        ";
    }

    protected string FileImage(string path, string alt, string className = "", string style = "")
    {
        string imageBase64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Mails/Templates/Images/", path)));
        return @$"
            <img src=""data:image/png;base64,{imageBase64}"" alt=""{alt}"" class=""rounded {className}"" style=""max-width: 100%; {style}"" />
        ";
    }

    protected string UrlImage(string url, string alt, string className = "", string style = "")
    {
        return @$"
            <img src=""{url}"" alt=""{alt}"" class=""rounded {className}"" style=""max-width: 100%; {style}"" />
        ";
    }

    protected string H1(string text)
    {
        return @$"
            <h1 class=""text-2xl font-bold py-3"">{text}</h1>
        ";
    }

    protected string H2(string text)
    {
        return @$"
            <h2 class=""text-xl font-bold py-2"">{text}</h2>
        ";
    }

    protected string Ul(string text)
    {
        return @$"
            <ul class=""list-disc pl-6 py-2"">{text}</ul>
        ";
    }

    protected string Ol(string text)
    {
        return @$"
            <ol class=""list-decimal pl-6 py-2"">{text}</ol>
        ";
    }

    protected string Li(string text)
    {
        return @$"
            <li class=""py-1 my-1"">{text}</li>
        ";
    }

    protected string StrikeThrough(string text)
    {
        return @$"
            <span class=""line-through"">{text}</span>
        ";
    }

    protected string JustifyBetween(string left, string right)
    {
        return @$"
            <div class=""flex justify-between"">
                <span class=""flex items-center justify-start"">{left}</span>
                <span class=""flex items-center justify-start"">{right}</span>
            </div>
        ";
    }

    protected string Hr()
    {
        return @"<hr class=""my-6 border-t border-gray-300"" />";
    }

    protected string Space()
    {
        return @"<div class=""h-6""></div>";
    }

    protected string Pluralize(string word, int count) => count == 1 ? word : $"{word.TrimEnd('y')}ies".Replace("iess", "ies").Replace("yies", "ies");

    protected string Pluralize(int count, string singular, string plural) => count == 1 ? singular : plural;

    protected string FormatBilling(decimal amount, string currency, bool includeCurrencyName = true)
    {
        var symbol = currency.ToUpper() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => currency.ToUpper() + " "
        };
        return $"{symbol}{amount:F2}{(includeCurrencyName ? " " + currency.ToUpper() : "")}";
    }

    public string InlineCss(string html)
    {
        string css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Mails/Templates/EmailTemplate.css"));
        Regex[] patternsToBeRemoved = [
            new Regex("--tw-ring-color: rgba\\(59, 130, 246, 0.5\\);"),
            new Regex("--tw-ring-color: rgb\\(59 130 246 / 0.5\\);"),
            new Regex("--tw-ring-inset: var\\(--tw-empty, \\);"),
            new Regex("--tw-ring-offset-color: #fff;"),
            new Regex("--tw-ring-offset-shadow: 0 0 #0000;"),
            new Regex("--tw-shadow-colored: 0 0 #0000;"),
            new Regex("--tw-ring-offset-width: 0px;"),
            new Regex("--tw-ring-shadow: 0 0 #0000;"),
            new Regex("--tw-shadow: 0 0 #0000;"),
            new Regex("--tw-bg-opacity: 1;"),
            new Regex("--tw-border-opacity: 1;"),
            new Regex("--tw-text-opacity: 1;"),
            new Regex(@"--tw(-(\w+))+\: ;"),
            new Regex(@"--tw(-(\w+))+\: \w+;"),
            new Regex(@"--tw(-(\w+))+\: #\w+;"),
            new Regex(@"--tw(-(\w+))+\: \d+;"),
        ];
        Regex[] patternsToBeReplaced = [
            new Regex("var\\(--tw-bg-opacity\\)"),
            new Regex("var\\(--tw-text-opacity\\)"),
            new Regex("var\\(--tw-border-opacity\\)"),
            new Regex("style=" + " +"),
        ];
        string inlineHtml = PreMailer.Net.PreMailer.MoveCssInline(
            @$"
            {html}
            ",
            css: css,
            stripIdAndClassAttributes: true,
            removeComments: true,
            useEmailFormatter: true
        ).Html;
        foreach (var pattern in patternsToBeRemoved)
        {
            inlineHtml = pattern.Replace(inlineHtml, "");
        }
        foreach (var pattern in patternsToBeReplaced)
        {
            inlineHtml = pattern.Replace(inlineHtml, pattern.ToString());
        }
        inlineHtml = inlineHtml.Replace("var(--tw-text-opacity, 1)", "1");
        inlineHtml = inlineHtml.Replace("var(--tw-border-opacity, 1)", "1");
        inlineHtml = inlineHtml.Replace("var(--tw-bg-opacity, 1)", "1");
        inlineHtml = inlineHtml.Replace("var(--tw-divide-opacity, 1)", "1");
        inlineHtml = inlineHtml.Replace("var(--tw-divide-y-reverse)", "0");
        inlineHtml = inlineHtml.Replace("var(--radius)", "0.25rem");
        inlineHtml = ReplaceRgbToHex(inlineHtml);
        inlineHtml = inlineHtml.Replace("calc(1px * calc(1 - 0))", "1px");
        inlineHtml = inlineHtml.Replace("calc(1px * 0)", "1px");
        return inlineHtml;
    }

    /// <summary>
    /// Replace all instances of rgb(r g b / 1) to #rgb
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    private static string ReplaceRgbToHex(string html)
    {
        var regex = new Regex(@"rgb\((\d+) (\d+) (\d+) / 1\)");
        var matches = regex.Matches(html);
        foreach (var match in matches.Cast<Match>())
        {
            int red = int.Parse(match.Groups[1].Value);
            int green = int.Parse(match.Groups[2].Value);
            int blue = int.Parse(match.Groups[3].Value);
            string hex = $"#{red:X2}{green:X2}{blue:X2}";
            html = html.Replace(match.Value, hex);
        }
        return html;
    }

    public class BackgroundColors
    {
        public static string Primary = "#BB017A";
        public static string Yellow = "#FFC300";
        public static string Green = "#17FF70";
        public static string Purple = "#803EC2";
        public static string Sage = "#84A98C";
        public static string Lavender = "#9575CD";
        public static string Lilac = "#E4B7E5";
        public static string Charcoal = "#1D1E2C";
        public static string Orange = "#FF5117";
        public static string Pink = "#FF1093";
        public static string Blue = "#17A6FF";
    }
}