namespace Cits.Permissions;

public class PermissionGroupModel
{
    private readonly List<PermissionModel> _permissions = new();
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public PermissionGroupModel AddPermission(
        string code,
        string displayName)
    {
        var permission = new PermissionModel
        {
            Code = code,
            DisplayName = displayName,
            GroupCode = Code
        };

        _permissions.Add(permission);
        return this;
    }

    public List<PermissionModel> GetPermissions()
    {
        return _permissions;
    }
}
