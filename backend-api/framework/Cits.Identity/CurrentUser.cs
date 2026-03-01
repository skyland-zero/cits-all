using System.Security.Claims;

namespace Cits;

public class CurrentUser : ICurrentUser
{
    private readonly IUserContextService _contextService;

    public CurrentUser(IUserContextService contextService)
    {
        _contextService = contextService;
    }

    public bool IsAuthenticated => Id != Guid.Empty;

    public Guid Id
    {
        get
        {
            var sid = _contextService.GetClaimsPrincipal()?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            Guid.TryParse(sid, out var outValue);
            return outValue;
        }
    }

    public string UserName => FindByClaimType(ClaimTypes.Name) ?? string.Empty;

    public string Surname => FindByClaimType(ClaimTypes.Surname) ?? string.Empty;

    private string? FindByClaimType(string claimType)
    {
        return _contextService.GetClaimsPrincipal()?.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
}