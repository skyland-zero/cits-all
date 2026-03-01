namespace MyApi.Domain.Models;

public class UserRoleCacheModel
{
    public Guid UserId { get; set; }
    public HashSet<string> RoleCodes { get; set; }
    public List<UserRoleItemCacheModel> Roles { get; set; }
}

public class UserRoleItemCacheModel
{
    public Guid Id { get; set; } 
    
    public string Name { get; set; }
    
    public string Code { get; set; }
}
