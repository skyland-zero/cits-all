using Cits.Dtos;

namespace Cits.SystemSettings;

public interface ISystemSettingAppService : ICrudAppService<SystemSettingDto, Guid, QuerySystemSettingDto, SystemSettingDto, CreateSystemSettingDto, UpdateSystemSettingDto>
{
    Task<List<SystemSettingGroupDto>> GetGroupsAsync();

    Task UpdateValueAsync(Guid id, UpdateSystemSettingValueDto input);
}
