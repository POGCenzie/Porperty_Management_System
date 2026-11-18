using Microsoft.AspNetCore.Identity;
using PropertyManagementSystem.Model;
using PropertyManagementSystem.Repository;

namespace PropertyManagementSystem.Services;

public sealed class AuthenticationService
{
    private readonly IUserRepository userRepository;
    private readonly PasswordHasher<User> passwordHasher = new();

    public AuthenticationService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByUsernameAsync(
            username.Trim(),
            cancellationToken);

        if (user is null)
        {
            return new AuthenticationResult(
                AuthenticationStatus.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return new AuthenticationResult(
                AuthenticationStatus.InactiveAccount);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            if (!PasswordPolicy.IsValid(password))
            {
                return new AuthenticationResult(
                    AuthenticationStatus.PasswordDoesNotMeetRequirements);
            }

            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await userRepository.UpdatePasswordHashAsync(
                user.UserId,
                user.PasswordHash,
                cancellationToken);

            return new AuthenticationResult(
                AuthenticationStatus.PasswordInitialized,
                user);
        }

        var verification = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return new AuthenticationResult(
                AuthenticationStatus.InvalidCredentials);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await userRepository.UpdatePasswordHashAsync(
                user.UserId,
                user.PasswordHash,
                cancellationToken);
        }

        return new AuthenticationResult(AuthenticationStatus.Success, user);
    }
}
