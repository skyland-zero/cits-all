namespace MyApi.Domain.Models;

public class UserAuthPointCacheModel
{
    public UserRoleCacheModel Roles { get; set; } = new();
    
    public HashSet<string> Points { get; set; } = new();
}
