using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.Identities.Dto;

public class ChangePasswordDto
{
    [MaxLength(500)]
    public string OldPassword { get; set; } = string.Empty;

    [MaxLength(500)]
    public string NewPassword { get; set; } = string.Empty;
}
