using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Services;

public enum AuthenticationStatus
{
    Success,
    PasswordInitialized,
    PasswordDoesNotMeetRequirements,
    InactiveAccount,
    InvalidCredentials
}

public sealed record AuthenticationResult(
    AuthenticationStatus Status,
    User? User = null);
