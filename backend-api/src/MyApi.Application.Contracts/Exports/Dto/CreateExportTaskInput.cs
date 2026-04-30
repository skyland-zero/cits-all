namespace MyApi.Application.Exports.Dto;

public class CreateExportTaskInput
{
    public string ModuleKey { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public Dictionary<string, object?> Query { get; set; } = new();

    public List<string> Fields { get; set; } = [];
}
