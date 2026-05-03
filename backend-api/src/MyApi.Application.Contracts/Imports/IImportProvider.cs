using Cits.DI;
using MyApi.Application.Imports.Dto;

namespace MyApi.Application.Imports;

public interface IImportProvider : IScopedService
{
    string ModuleKey { get; }

    string ModuleName { get; }

    IReadOnlyList<ImportTemplateColumnDto> Columns { get; }

    Task<ImportProviderResult> ImportAsync(Stream stream, CancellationToken cancellationToken = default);
}
