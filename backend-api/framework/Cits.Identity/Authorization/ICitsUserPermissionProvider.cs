namespace Cits.Authorization;

public interface ICitsUserPermissionProvider
{
    ValueTask<HashSet<string>> GetAuthPointsAsync(Guid userId);
}