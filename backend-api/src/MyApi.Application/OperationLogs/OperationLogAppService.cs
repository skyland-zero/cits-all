using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using Cits;
using Cits.OperationLogs;
using FreeSql;
using MyApi.Application.OperationLogs.Dto;

namespace MyApi.Application.OperationLogs;

public class OperationLogAppService : IOperationLogAppService
{
    private static readonly Expression<Func<OperationLog, OperationLogCursorItemDto>> CursorItemSelector = x => new OperationLogCursorItemDto
    {
        Id = x.Id,
        Module = x.Module,
        OperationType = x.OperationType,
        OperatorId = x.OperatorId,
        OperatorName = x.OperatorName,
        DepartmentPath = x.DepartmentPath,
        OperationIp = x.OperationIp,
        OperationLocation = x.OperationLocation,
        Status = x.Status,
        OperationTime = x.OperationTime,
        ElapsedMilliseconds = x.ElapsedMilliseconds,
        RequestPath = x.RequestPath,
        RequestMethod = x.RequestMethod
    };

    private readonly IFreeSql _freeSql;

    public OperationLogAppService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public async Task<OperationLogCursorResultDto> GetListAsync(GetOperationLogsInput input)
    {
        var queryContext = ResolveQueryContext(input);
        var takeCount = Math.Min(input.MaxResultCount, 200);
        var query = BuildQuery(input, queryContext.StartTime, queryContext.EndTime);

        if (queryContext.CursorTime.HasValue && queryContext.CursorId.HasValue)
        {
            query = ApplyCursor(query, queryContext.CursorTime.Value, queryContext.CursorId.Value);
        }
        else if (queryContext.CursorTime.HasValue)
        {
            query = query.Where(x => x.OperationTime < queryContext.CursorTime.Value);
        }

        var items = await query
            .OrderByDescending(x => x.OperationTime)
            .OrderByDescending(x => x.Id)
            .Take(takeCount + 1)
            .ToListAsync(CursorItemSelector);

        var hasMore = items.Count > takeCount;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var lastItem = items.LastOrDefault();

        return new OperationLogCursorResultDto
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore && lastItem is not null
                ? EncodeCursor(new OperationLogCursorPayload(
                    lastItem.OperationTime.Ticks,
                    lastItem.Id,
                    queryContext.StartTime.Ticks,
                    queryContext.EndTime.Ticks))
                : null,
            NextCursorTime = hasMore ? lastItem?.OperationTime : null,
            NextCursorId = hasMore ? lastItem?.Id : null
        };
    }

    private OperationLogQueryContext ResolveQueryContext(GetOperationLogsInput input)
    {
        var cursor = DecodeCursor(input.Cursor);
        if (cursor is not null)
        {
            return new OperationLogQueryContext(
                new DateTime(cursor.StartTimeTicks),
                new DateTime(cursor.EndTimeTicks),
                new DateTime(cursor.OperationTimeTicks),
                cursor.Id);
        }

        var endTime = input.EndTime ?? DateTime.Now;
        var startTime = input.StartTime ?? endTime.AddDays(-7);

        return new OperationLogQueryContext(startTime, endTime, input.CursorTime, input.CursorId);
    }

    private ISelect<OperationLog> ApplyCursor(ISelect<OperationLog> query, DateTime cursorTime, Guid cursorId)
    {
        var parameters = new { cursorTime, cursorId };
        return _freeSql.Ado.DataType switch
        {
            DataType.Sqlite or DataType.PostgreSQL => query.Where(
                "(\"OperationTime\" < @cursorTime OR (\"OperationTime\" = @cursorTime AND \"Id\" < @cursorId))",
                parameters),
            _ => throw new UserFriendlyException($"操作日志游标暂不支持当前数据库类型：{_freeSql.Ado.DataType}")
        };
    }

    private ISelect<OperationLog> BuildQuery(GetOperationLogsInput input, DateTime startTime, DateTime endTime)
    {
        var query = _freeSql.Select<OperationLog>()
            .Where(x => x.OperationTime >= startTime && x.OperationTime <= endTime)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Module), x => x.Module == input.Module)
            .WhereIf(!string.IsNullOrWhiteSpace(input.OperationType), x => x.OperationType == input.OperationType)
            .WhereIf(!string.IsNullOrWhiteSpace(input.OperatorId), x => x.OperatorId == input.OperatorId);

        if (input.Status.HasValue)
        {
            var status = input.Status.Value;
            query = query.Where(x => x.Status == status);
        }

        return query;
    }

    private static string EncodeCursor(OperationLogCursorPayload cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static OperationLogCursorPayload? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var base64 = cursor.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return JsonSerializer.Deserialize<OperationLogCursorPayload>(json)
                   ?? throw new UserFriendlyException("操作日志游标无效");
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException)
        {
            throw new UserFriendlyException("操作日志游标无效", ex);
        }
    }

    public async Task<OperationLogDetailDto?> GetAsync(Guid id)
    {
        return await _freeSql.Select<OperationLog>()
            .Where(x => x.Id == id)
            .ToOneAsync(x => new OperationLogDetailDto
            {
                Id = x.Id,
                Module = x.Module,
                OperationType = x.OperationType,
                OperatorId = x.OperatorId,
                OperatorName = x.OperatorName,
                DepartmentPath = x.DepartmentPath,
                OperationIp = x.OperationIp,
                OperationLocation = x.OperationLocation,
                Status = x.Status,
                OperationTime = x.OperationTime,
                ElapsedMilliseconds = x.ElapsedMilliseconds,
                RequestPath = x.RequestPath,
                RequestMethod = x.RequestMethod,
                RequestParameters = x.RequestParameters,
                ResponseParameters = x.ResponseParameters,
                ErrorMessage = x.ErrorMessage
            });
    }

    private sealed record OperationLogQueryContext(DateTime StartTime, DateTime EndTime, DateTime? CursorTime, Guid? CursorId);

    private sealed record OperationLogCursorPayload(long OperationTimeTicks, Guid Id, long StartTimeTicks, long EndTimeTicks);
}
