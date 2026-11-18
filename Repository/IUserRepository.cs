using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Repository;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(
        string username,
        int excludingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task UpdatePasswordHashAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken = default);

    Task<User> UpdateAccountAsync(
        int userId,
        string username,
        string displayName,
        string? passwordHash,
        CancellationToken cancellationToken = default);

    Task SetActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
