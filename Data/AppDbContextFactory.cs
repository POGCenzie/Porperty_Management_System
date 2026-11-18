using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PropertyManagementSystem.Configuration;

namespace PropertyManagementSystem.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(AppConfiguration.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }
}
