using Cits.DI;
using Cits.Dtos;

namespace Cits;

public interface ICrudAppService<TGetOutputDto, in TKey, in TGetListInput, TGetListOutputDto, in TCreateInput,
    in TUpdateInput> : IApplicationService
{
    Task<TGetOutputDto> GetAsync(TKey id);

    Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input);

    Task CreateAsync(TCreateInput input);

    Task UpdateAsync(TKey id, TUpdateInput input);

    Task DeleteAsync(TKey id);
}