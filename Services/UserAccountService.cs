using Microsoft.AspNetCore.Identity;
using PropertyManagementSystem.Constants;
using PropertyManagementSystem.Model;
using PropertyManagementSystem.Repository;

namespace PropertyManagementSystem.Services;

public sealed class UserAccountService
{
    private readonly IUserRepository userRepository;
    private readonly PasswordHasher<User> passwordHasher = new();

    public UserAccountService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<AccountUpdateResult> UpdateCurrentUserAsync(
        User currentUser,
        string displayName,
        string username,
        string currentPassword,
        string? newPassword,
        CancellationToken cancellationToken = default)
    {
        displayName = displayName.Trim();
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 150)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidDisplayName);
        }

        if (string.IsNullOrWhiteSpace(username) || username.Length > 100)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidUsername);
        }

        var storedUser = await userRepository.FindByIdAsync(
            currentUser.UserId,
            cancellationToken);
        if (storedUser is null || string.IsNullOrWhiteSpace(storedUser.PasswordHash))
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidCurrentPassword);
        }

        var verification = passwordHasher.VerifyHashedPassword(
            storedUser,
            storedUser.PasswordHash,
            currentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidCurrentPassword);
        }

        if (await userRepository.UsernameExistsAsync(
            username,
            storedUser.UserId,
            cancellationToken))
        {
            return new AccountUpdateResult(AccountUpdateStatus.UsernameAlreadyExists);
        }

        string? updatedPasswordHash = null;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (!PasswordPolicy.IsValid(newPassword))
            {
                return new AccountUpdateResult(
                    AccountUpdateStatus.PasswordDoesNotMeetRequirements);
            }

            storedUser.Username = username;
            storedUser.DisplayName = displayName;
            updatedPasswordHash = passwordHasher.HashPassword(
                storedUser,
                newPassword);
        }
        else if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            updatedPasswordHash = passwordHasher.HashPassword(
                storedUser,
                currentPassword);
        }

        var updatedUser = await userRepository.UpdateAccountAsync(
            storedUser.UserId,
            username,
            displayName,
            updatedPasswordHash,
            cancellationToken);
        return new AccountUpdateResult(AccountUpdateStatus.Success, updatedUser);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(
        User administrator,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdministrator(administrator))
        {
            return Array.Empty<User>();
        }

        return await userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<AccountUpdateResult> UpdateUserAsAdminAsync(
        User administrator,
        int targetUserId,
        string displayName,
        string username,
        string administratorPassword,
        string? newPassword,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdministrator(administrator))
        {
            return new AccountUpdateResult(AccountUpdateStatus.Unauthorized);
        }

        displayName = displayName.Trim();
        username = username.Trim();
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 150)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidDisplayName);
        }

        if (string.IsNullOrWhiteSpace(username) || username.Length > 100)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidUsername);
        }

        var storedAdministrator = await userRepository.FindByIdAsync(
            administrator.UserId,
            cancellationToken);
        if (storedAdministrator is null ||
            string.IsNullOrWhiteSpace(storedAdministrator.PasswordHash) ||
            passwordHasher.VerifyHashedPassword(
                storedAdministrator,
                storedAdministrator.PasswordHash,
                administratorPassword) == PasswordVerificationResult.Failed)
        {
            return new AccountUpdateResult(AccountUpdateStatus.InvalidCurrentPassword);
        }

        var targetUser = await userRepository.FindByIdAsync(
            targetUserId,
            cancellationToken);
        if (targetUser is null)
        {
            return new AccountUpdateResult(AccountUpdateStatus.Unauthorized);
        }

        if (await userRepository.UsernameExistsAsync(
            username,
            targetUserId,
            cancellationToken))
        {
            return new AccountUpdateResult(AccountUpdateStatus.UsernameAlreadyExists);
        }

        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (!PasswordPolicy.IsValid(newPassword))
            {
                return new AccountUpdateResult(
                    AccountUpdateStatus.PasswordDoesNotMeetRequirements);
            }

            targetUser.Username = username;
            targetUser.DisplayName = displayName;
            passwordHash = passwordHasher.HashPassword(targetUser, newPassword);
        }

        var updatedUser = await userRepository.UpdateAccountAsync(
            targetUserId,
            username,
            displayName,
            passwordHash,
            cancellationToken);
        return new AccountUpdateResult(AccountUpdateStatus.Success, updatedUser);
    }

    public async Task<bool> SetUserActiveAsync(
        User administrator,
        int targetUserId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdministrator(administrator) ||
            (!isActive && administrator.UserId == targetUserId))
        {
            return false;
        }

        await userRepository.SetActiveAsync(
            targetUserId,
            isActive,
            cancellationToken);
        return true;
    }

    private static bool IsAdministrator(User user) =>
        user.IsActive &&
        string.Equals(
            user.UserGroup,
            UserGroups.Admin,
            StringComparison.OrdinalIgnoreCase);
}
