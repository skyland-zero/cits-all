namespace MyApi.Domain.Models;

public class UserRoleMenuCacheModel
{
    public UserRoleCacheModel Roles { get; set; } = new();
    
    public List<MenuCacheModel> Menus { get; set; } = new();
}
