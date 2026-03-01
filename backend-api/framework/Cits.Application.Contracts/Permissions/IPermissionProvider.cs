namespace Cits.Permissions;

public interface IPermissionProvider
{
    void Define(PermissionDefinitionContext context);
}