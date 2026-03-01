namespace MyApi.Domain.Models;

public class UserRoleMenuCacheModel
{
    public UserRoleCacheModel Roles { get; set; }
    
    public List<MenuCacheModel> Menus { get; set; }
}