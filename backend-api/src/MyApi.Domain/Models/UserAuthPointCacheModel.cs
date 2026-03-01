namespace MyApi.Domain.Models;

public class UserAuthPointCacheModel
{
    public UserRoleCacheModel Roles { get; set; }
    
    public HashSet<string> Points { get; set; }
}