using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class GetIdentityMenusInput : PagedRequestDto
{
    public Guid? ParentId { get; set; }
}