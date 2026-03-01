using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Cits.Authorization;

public class CitsPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CitsPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName)) return _fallbackPolicyProvider.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new CitsPermissionRequirement(policyName));
        return Task.FromResult(policy.Build())!;
    }
}