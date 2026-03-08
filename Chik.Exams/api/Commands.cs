using System.CommandLine;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mykeels.CSharpRepl;

namespace Chik.Exams;

public static class Commands
{
    public static RootCommand Setup()
    {
        var rootCommand = new RootCommand("under4games.api");
        rootCommand.TreatUnmatchedTokensAsErrors = false;
        rootCommand.AddGlobalOption(
            new Option<bool>("--db", () => true, "Connect to the database (default: true)")
        );

        rootCommand.SetupREPLCommand();
        return rootCommand;
    }

    private static void SetupREPLCommand(this RootCommand rootCommand)
    {
        var replCommand = new Command("repl", "Launch a REPL");

        replCommand.SetHandler(async () =>
        {
            var logger = Provider.GetRequiredService<ILogger<Program>>();
            await Repl.Run(
                new CSharpRepl.Services.Configuration(
                    usings: [
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "Chik.Exams",
                        "Chik.Exams.Api",
                    ],
                    applicationName: "Chik.Exams.Api"
                ),
                onLoad: (rosylnServices) => {
                    rosylnServices
                        .EvaluateAsync(
                            "using static Chik.Exams.ScriptGlobals;",
                            cancellationToken: CancellationToken.None
                        )
                        .ConfigureAwait(false);
                    string cwd = AppContext.BaseDirectory;
                    string logFilePath = Path.Combine(cwd, "Chik.Exams.Api.log");
                    Console.WriteLine($"Logs: {logFilePath}");
                }
            );
        });

        rootCommand.AddCommand(replCommand);
    }
}