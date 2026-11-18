namespace PropertyManagementSystem.Model;

public sealed class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string UserGroup { get; set; } = Constants.UserGroups.User;

    public bool IsActive { get; set; } = true;

    public string? PasswordHash { get; set; }

    public DateTime LastUpdatedDate { get; set; }
}
