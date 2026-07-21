using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BasketService.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Allows `dotnet ef` commands to create a DbContext instance without running the full app.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BasketDbContext>
{
    public BasketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BasketDbContext>();

        // Use the local development connection string
        var connectionString = "Host=localhost;Port=5432;Database=basketdb;Username=retail;Password=retail_dev_pw";
        optionsBuilder.UseNpgsql(connectionString);

        return new BasketDbContext(optionsBuilder.Options);
    }
}
