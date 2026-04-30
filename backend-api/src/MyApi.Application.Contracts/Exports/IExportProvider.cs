using Cits.DI;
using MyApi.Application.Exports.Dto;

namespace MyApi.Application.Exports;

public interface IExportProvider : IScopedService
{
    string ModuleKey { get; }

    string ModuleName { get; }

    IReadOnlyList<ExportFieldDto> Fields { get; }

    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GetRowsAsync(
        string queryJson,
        CancellationToken cancellationToken = default);
}
