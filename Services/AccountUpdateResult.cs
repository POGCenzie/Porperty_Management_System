using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Services;

public enum AccountUpdateStatus
{
    Success,
    InvalidDisplayName,
    InvalidUsername,
    InvalidCurrentPassword,
    UsernameAlreadyExists,
    PasswordDoesNotMeetRequirements,
    Unauthorized
}

public sealed record AccountUpdateResult(
    AccountUpdateStatus Status,
    User? User = null);
