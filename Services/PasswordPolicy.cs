namespace PropertyManagementSystem.Services;

public static class PasswordPolicy
{
    public const string RequirementMessage =
        "Password must contain at least 8 characters, including uppercase, lowercase, number and special character.";

    public static bool IsValid(string password)
    {
        return password.Length >= 8 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit) &&
            password.Any(character => !char.IsLetterOrDigit(character));
    }
}
