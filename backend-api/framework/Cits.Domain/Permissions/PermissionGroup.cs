namespace Cits.Permissions;

public class PermissionGroup
{
    private readonly List<Permission> _permissions = new();
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Code { get; set; }
    public string DisplayName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public PermissionGroup AddPermission(
        string code,
        string displayName)
    {
        var permission = new Permission
        {
            Code = code,
            DisplayName = displayName,
            GroupCode = Code
        };

        _permissions.Add(permission);
        return this;
    }
}