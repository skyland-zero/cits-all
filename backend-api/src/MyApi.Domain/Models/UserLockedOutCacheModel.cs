namespace MyApi.Domain.Models;

public sealed class UserLockedOutCacheModel
{
    // ReSharper disable once UnusedMember.Global
    public UserLockedOutCacheModel()
    {
    }

    public UserLockedOutCacheModel(string username)
    {
        Username = username;
    }

    public UserLockedOutCacheModel(string username, DateTimeOffset? lockoutEnd)
    {
        Username = username;
        LockoutEnd = lockoutEnd;
    }

    public string Username { get; set; } = string.Empty;
    public DateTimeOffset? LockoutEnd { get; set; }
}
