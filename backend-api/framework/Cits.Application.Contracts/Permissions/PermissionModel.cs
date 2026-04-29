namespace Cits.Permissions;

public class PermissionModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Code { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? GroupCode { get; set; }
    public bool IsEnabled { get; set; } = true;
}
