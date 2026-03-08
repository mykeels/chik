using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Chik.Exams.Data;

namespace Chik.Exams.Tests;

[SetUpFixture]
public class TestSetup
{
    private static IDbContextFactory<ChikExamsDbContext>? _sharedFactory;
    private static readonly object _lock = new object();

    public static Func<IDbContextFactory<ChikExamsDbContext>> DbContextFactory { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Create a shared factory with a fixed database name
        // All tests will use the same in-memory database
        // IMPORTANT: Tests must clean up in [SetUp] methods to avoid interference
        EnsureDatabaseReset();
    }

    public static void EnsureDatabaseReset()
    {
        // Reset the shared database - all tests using this factory will see the reset
        lock (_lock)
        {
            if (_sharedFactory is null || DbContextFactory is null)
            {
                var options = new DbContextOptionsBuilder<ChikExamsDbContext>()
                    .UseInMemoryDatabase(databaseName: "ChikExams_Test")
                    .Options;
                _sharedFactory = new PooledDbContextFactory<ChikExamsDbContext>(options, 1);
                
                DbContextFactory = () => _sharedFactory;
            }
            using var dbContext = _sharedFactory!.CreateDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
    }
}