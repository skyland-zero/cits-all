namespace MyApi.Application.Exports.Dto;

public sealed record ExportFieldDto(string Key, string Label, bool Selected = true);
