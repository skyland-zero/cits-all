using Cits.Dtos;
using FreeSql;

namespace Cits;

public static class FreeSqlExtension
{
    public static ISelect<T1> PageBy<T1>(this ISelect<T1> select, PagedRequestDto paged)
    {
        return select.Skip(paged.SkipCount).Take(paged.MaxResultCount);
    }
}