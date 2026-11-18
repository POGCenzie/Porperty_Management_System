using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Services;

public sealed class CurrentUserSession
{
    public User? User { get; private set; }

    public bool IsAuthenticated => User is not null;

    public void Start(User user)
    {
        User = user;
    }

    public void Clear()
    {
        User = null;
    }
}
