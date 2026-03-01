namespace Cits.Permissions;

public class PermissionDefinitionContext
{
    private readonly List<PermissionGroupModel> _groups = new();

    public PermissionGroupModel AddGroup(
        string code,
        string displayName)
    {
        var permissionGroup = new PermissionGroupModel
        {
            Code = code,
            DisplayName = displayName
        };

        _groups.Add(permissionGroup);
        return permissionGroup;
    }


    public IReadOnlyList<PermissionGroupModel> GetGroups()
    {
        return _groups.AsReadOnly();
    }
}