using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Cits;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? GetClaimsPrincipal()
    {
        return _httpContextAccessor?.HttpContext?.User;
    }
}