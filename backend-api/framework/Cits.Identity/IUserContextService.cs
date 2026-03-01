using System.Security.Claims;

namespace Cits;

public interface IUserContextService
{
    ClaimsPrincipal? GetClaimsPrincipal();
}