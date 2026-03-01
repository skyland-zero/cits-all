using Microsoft.AspNetCore.Mvc;

namespace Cits;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}