namespace MyApi.Domain.Models;

public class UserRoleCacheModel
{
    public Guid UserId { get; set; }
    public HashSet<string> RoleCodes { get; set; } = new();
    public List<UserRoleItemCacheModel> Roles { get; set; } = new();
}

public class UserRoleItemCacheModel
{
    public Guid Id { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
}
