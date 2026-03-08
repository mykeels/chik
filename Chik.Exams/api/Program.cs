using System.CommandLine;
using Chik.Exams;
using Chik.Exams.Api;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

var startup = new Startup(builder.Configuration, args);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

await startup.Configure(app);

startup.RootCommand.SetHandler(() => app.Run());
startup.RootCommand.Invoke(args);
