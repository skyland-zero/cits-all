using Cits;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IUserAppService : ICrudAppService<
    UserDto,
    Guid,
    GetUsersInput,
    UserDto,
    UserCreateDto,
    UserUpdateDto>
{
    Task ResetPasswordAsync(Guid id, UserResetPasswordDto input);
}