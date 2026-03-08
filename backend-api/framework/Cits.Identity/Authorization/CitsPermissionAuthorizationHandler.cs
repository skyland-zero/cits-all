using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cits.Authorization;

// This class contains logic for determining whether MinimumAgeRequirements in authorization
// policies are satisfied or not
public class CitsPermissionAuthorizationHandler : AuthorizationHandler<CitsPermissionRequirement>
{
    private readonly ILogger<CitsPermissionAuthorizationHandler> _logger;
    private readonly ICitsUserPermissionProvider _userPermissionProvider;
    private readonly PermissionOptions _options;

    public CitsPermissionAuthorizationHandler(ILogger<CitsPermissionAuthorizationHandler> logger,
        ICitsUserPermissionProvider userPermissionProvider,
        IOptions<PermissionOptions> options)
    {
        _logger = logger;
        _userPermissionProvider = userPermissionProvider;
        _options = options.Value;
    }

    // Check whether a given MinimumAgeRequirement is satisfied or not for a particular context
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CitsPermissionRequirement requirement)
    {
        if (!_options.Enabled)
        {
            context.Succeed(requirement);
            return;
        }

        // Log as a warning so that it's very clear in sample output which authorization policies
        // (and requirements/handlers) are in use
        _logger.LogWarning("权限点判断 {age}", requirement.Code);

        // Check the user's age
        var sid = context.User.FindFirst(c => c.Type == ClaimTypes.Sid);
        if (sid != null && Guid.TryParse(sid.Value, out var uid))
        {
            var points = await _userPermissionProvider.GetAuthPointsAsync(uid);
            if (points.Contains(requirement.Code))
            {
                context.Succeed(requirement);
            }
        }
    }
}