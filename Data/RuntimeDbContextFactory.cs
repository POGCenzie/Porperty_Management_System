using Microsoft.EntityFrameworkCore;

namespace PropertyManagementSystem.Data;

public sealed class RuntimeDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> options;

    public RuntimeDbContextFactory(string connectionString)
    {
        options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    public AppDbContext CreateDbContext()
    {
        return new AppDbContext(options);
    }
}
