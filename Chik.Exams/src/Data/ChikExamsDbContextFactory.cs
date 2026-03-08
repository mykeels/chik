using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chik.Exams.Data;

internal class ChikExamsDbContextFactory : IDesignTimeDbContextFactory<ChikExamsDbContext>
{
    public ChikExamsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChikExamsDbContext>();
        
        // Use SQL Server with a connection string for design-time
        // You can modify this connection string as needed for your development environment
        optionsBuilder.UseNpgsql(ChikExamsDbContext.GetConnectionString(password: "postgres"));
        
        return new ChikExamsDbContext(optionsBuilder.Options);
    }
} 