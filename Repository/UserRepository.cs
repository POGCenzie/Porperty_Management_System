using Microsoft.EntityFrameworkCore;
using PropertyManagementSystem.Data;
using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Repository;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public UserRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<User?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        return await database.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Username == username,
                cancellationToken);
    }

    public async Task<User?> FindByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        return await database.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.UserId == userId,
                cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(
        string username,
        int excludingUserId,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        return await database.Users.AnyAsync(
            user => user.Username == username && user.UserId != excludingUserId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        return await database.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePasswordHashAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        var updatedRows = await database.Users
            .Where(user => user.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.PasswordHash, passwordHash)
                    .SetProperty(user => user.LastUpdatedDate, DateTime.UtcNow),
                cancellationToken);

        if (updatedRows != 1)
        {
            throw new InvalidOperationException("The user could not be updated.");
        }
    }

    public async Task<User> UpdateAccountAsync(
        int userId,
        string username,
        string displayName,
        string? passwordHash,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        var user = await database.Users.SingleOrDefaultAsync(
            item => item.UserId == userId,
            cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("The current user no longer exists.");
        }

        user.Username = username;
        user.DisplayName = displayName;
        user.LastUpdatedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(passwordHash))
        {
            user.PasswordHash = passwordHash;
        }

        await database.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task SetActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(
            cancellationToken);

        var updatedRows = await database.Users
            .Where(user => user.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.IsActive, isActive)
                    .SetProperty(user => user.LastUpdatedDate, DateTime.UtcNow),
                cancellationToken);
        if (updatedRows != 1)
        {
            throw new InvalidOperationException("The user could not be updated.");
        }
    }
}
