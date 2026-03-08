namespace Chik.Exams.Mails;

public static class MailTemplateSamplesRenderer
{
    /// <summary>
    /// Using reflection, find all classes that implement IEmailTemplate<TData>
    /// Using its SampleData property, render the html for each sample data
    /// Save the html to a file in ../Samples directory
    /// </summary>
    /// <returns></returns>
    public static async Task Render(
        ILogger<IEmailTemplateSampleDataRenderer>? logger = null
    )
    {
        logger ??= Provider.Instance.GetRequiredService<ILogger<IEmailTemplateSampleDataRenderer>>();
        string outputDirectory = Path.Combine(AppContext.BaseDirectory, "Mails/Templates/Samples");
        logger.LogInformation("Output directory: {OutputDirectory}", outputDirectory);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        var templates = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IEmailTemplateSampleDataRenderer).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        var sb = new System.Text.StringBuilder();
        
        logger.LogInformation("Rendering {Templates} templates", templates.Count);
        foreach (var template in templates)
        {
            logger.LogInformation("Rendering {Template} template", template.Name);
            var instance = Provider.Instance.GetRequiredService(template) as IEmailTemplateSampleDataRenderer;
            if (instance is null)
            {
                continue;
            }
            var fileNames = await instance.Render(outputDirectory);
            foreach (var fileName in fileNames)
            {
                sb.AppendLine($@"<div><a href=""./{fileName}"">{fileName}</a></div>");
            }
        }
        File.WriteAllText(Path.Combine(outputDirectory, "index.html"), sb.ToString());
    }
}