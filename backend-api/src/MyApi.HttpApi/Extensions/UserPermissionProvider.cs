using Cits.Authorization;
using MyApi.Domain.Identities;

namespace MyApi.HttpApi.Extensions;

public class UserPermissionProvider : ICitsUserPermissionProvider
{
    private readonly UserPermissionManager _userPermissionManager;

    public UserPermissionProvider(UserPermissionManager userPermissionManager)
    {
        _userPermissionManager = userPermissionManager;
    }

    public async ValueTask<HashSet<string>> GetAuthPointsAsync(Guid userId)
    {
        var res = await _userPermissionManager.GetUserAuthPointAsync(userId);
        return res.Points;
    }
}