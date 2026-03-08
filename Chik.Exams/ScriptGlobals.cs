using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TextCopy;
using Chik.Exams.Data;
using ZiggyCreatures.Caching.Fusion;

namespace Chik.Exams;

public static class ScriptGlobals
{
    public static IUserService userService => Provider.GetRequiredService<IUserService>();
    public static ILoginService loginService => Provider.GetRequiredService<ILoginService>();
    public static RemoteEnvironment remoteEnvironment => Provider.GetRequiredService<RemoteEnvironment>();
    public static ChikExamsDbContext dbContext => Provider.GetRequiredService<ChikExamsDbContext>();
    public static ILogger logger => Provider.GetRequiredService<ILogger<ScriptGlobalsLogger>>();
    public static IConfiguration configuration => Provider.GetRequiredService<IConfiguration>();
    public static IFusionCache cacheManager => Provider.GetRequiredService<IFusionCache>();

    private static Auth? _admin;
    public static Auth admin
    {
        get
        {
            if (_admin is null)
            {
                _admin = GetAdmin().Result;
            }
            return _admin;
        }
    }

    public static async Task<Auth> GetAdmin()
    {
        var user = await userService.Repository.Search(new User.Filter(Roles: new List<UserRole> { UserRole.Admin }.ToInt32()));
        if (user is null || user.Items.Count == 0)
        {
            throw new KeyNotFoundException("Admin user not found");
        }
        _admin = (Auth)user.Items.FirstOrDefault()!;
        var logins = await loginService.Repository.GetLastLogin(_admin.Id);
        _admin.LastLogin = logins?.CreatedAt;
        return _admin;
    }

    public static async Task Migrate()
    {
        using (var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            logger.LogInformation("Migrating database...");
            var dbContext = scope.ServiceProvider.GetRequiredService<ChikExamsDbContext>();
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrated successfully");
        }
    }

    public static T Await<T>(this Task<T> task)
    {
        return task.Result;
    }

    /// <summary>
    /// Iterate over a paginated list of items and perform an action on each item.
    /// <example>    
    /// <code language="csharp">
    /// await userService.Repository.Search().Iterate(
    ///     OnUser(async auth => {
    ///         logger.Info(auth.Email, new { auth.Id, auth.Name, auth.LastLogin });
    ///         await Task.CompletedTask;
    ///     })
    /// );
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of the items to iterate over.</typeparam>
    /// <param name="paginated">The paginated list of items to iterate over.</param>
    /// <param name="action">The action to perform on each item.</param>
    /// <param name="next">The next function to call to get the next page of items.</param>
    /// <param name="filter">The filter function to determine if the item should be processed.</param>
    public static async Task Iterate<T>(
        this IEnumerable<T> items,
        Func<T, Task>? action = null,
        Func<T, Task<bool>>? filter = null,
        bool displayOptions = true
    )
    {
        action ??= async (item) => await Task.CompletedTask;
        filter ??= async (item) => await Task.FromResult(true);
        int index = 0;
        foreach (var item in items)
        {
            if (!await filter(item))
            {
                continue;
            }
            logger.Info(index.ToString(), new { item });
            await action(item);
            index++;
            if (displayOptions)
            {
                DisplayOptions(item);
            }
        }
    }

    public static async Task<Paginated<Auth>> Users(User.Filter filter)
    {
        var paginated = await userService.Repository.Search(filter);
        var paginatedUsers = new Paginated<Auth>(
            paginated.Items.Select(u => (Auth)u!).ToList(),
            paginated.TotalCount,
            paginated.Page,
            paginated.PageSize,
            async options => await Task.FromResult<Paginated<Auth>>(null!)
        );
        foreach (var user in paginatedUsers.Items)
        {
            var logins = await loginService.Repository.GetLastLogin(user.Id);
            user.LastLogin = logins?.CreatedAt;
        }
        return paginatedUsers;
    }

    public static Func<UserDbo, int, Task> OnUser(
        Func<Auth, Task> action
    )
    {
        return async (UserDbo userDbo, int index) => {
            var auth = (Auth)userDbo!;
            var logins = await loginService.Repository.GetLastLogin(auth.Id);
            auth.LastLogin = logins?.CreatedAt;
            await action(auth);
        };
    }

    public static async Task ForEachUser(
        Func<Auth, Task> action,
        User.Filter? filter = null
    )
    {
        int page = 1;
        const int pageSize = 100;
        var paginationOptions = new PaginationOptions(page, pageSize);
        bool hasNextPage;
        int userCount = 0;
        do
        {
            var paginated = await userService.Repository.Search(filter, paginationOptions);
            userCount += paginated.Items.Count;
            hasNextPage = paginated.NextPage > paginated.Page;
            foreach (var user in paginated.Items)
            {
                if (user is null)
                {
                    continue;
                }
                var auth = (Auth)user!;
                var logins = await loginService.Repository.GetLastLogin(auth.Id);
                auth.LastLogin = logins?.CreatedAt;
                await action(auth);
            }
            page = paginated.NextPage;
            paginationOptions = new PaginationOptions(page, pageSize);
        }
        while (hasNextPage);
        logger.Info($"Processed {userCount} users", new { userCount, filter });
    }

    public static void Continue()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public static void ConfirmContinue()
    {
        Console.WriteLine("Are you sure you want to continue? (y/n)");
        var input = Console.ReadLine();
        if (input != "y")
        {
            throw new OperationCanceledException("Operation cancelled");
        }
    }

    public static void DisplayOptions<T>(this T? data)
    {
        while (true)
        {
            AnsiConsole.MarkupLine("[grey][[c]][/] Continue  [grey][[p]][/] Pretty view  [grey][[a]][/] Abort  [grey][[x]][/] Copy JSON");
            
            var key = Console.ReadKey(intercept: true);
            Console.WriteLine();
            
            switch (key.KeyChar)
            {
                case 'c':
                case 'C':
                    return;
                
                case 'p':
                case 'P':
                    Pretty(data);
                    break;
                
                case 'a':
                case 'A':
                    throw new OperationCanceledException("Operation aborted by user");
                
                case 'x':
                case 'X':
                    CopyJson(data);
                    break;
                
                default:
                    AnsiConsole.MarkupLine("[yellow]Invalid option. Please press c, p, a, or x.[/]");
                    break;
            }
        }
    }

    public static void Pretty<T>(this T? data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        AnsiConsole.Write(new Spectre.Console.Json.JsonText(json));
        AnsiConsole.WriteLine();
    }

    public static void CopyJson<T>(this T? data)
    {
        var jsonToCopy = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        ClipboardService.SetText(jsonToCopy);
        AnsiConsole.MarkupLine("[green]JSON copied to clipboard![/]");
    }

    public static async Task LastLoginReport()
    {
        var report = new List<(string Username, DateTime? LastLogin)>();
        var noLoginReport = new List<(string Username, DateTime? LastLogin)>();
        await ForEachUser(async user =>
        {
            if (user.LastLogin is not null)
            {
                report.Add((user.Username, user.LastLogin.Value));
            }
            else
            {
                noLoginReport.Add((user.Username, user.LastLogin));
            }
            await Task.CompletedTask;
        });
        report.OrderByDescending(r => r.LastLogin).ToList().ForEach(r =>
        {
            logger.Info($"{r.Username} => {r.LastLogin}");
        });
        noLoginReport.OrderBy(r => r.Username).ToList().ForEach(r =>
        {
            logger.Info($"{r.Username} => No login");
        });
    }

    public static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
}

public class ScriptGlobalsLogger { }