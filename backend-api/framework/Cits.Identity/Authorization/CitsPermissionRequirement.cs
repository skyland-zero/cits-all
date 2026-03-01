using Microsoft.AspNetCore.Authorization;

namespace Cits.Authorization;

public class CitsPermissionRequirement(string code) : IAuthorizationRequirement
{
    public string Code { get; private set; } = code;
}