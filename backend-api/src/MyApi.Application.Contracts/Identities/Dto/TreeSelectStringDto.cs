namespace MyApi.Application.Identities.Dto;

public class TreeSelectStringDto
{
    public string? ParentId { get; set; }
    public string Value { get; set; }
    public string? Label { get; set; }
    public List<TreeSelectStringDto>? Children { get; set; }
    public int Sort { get; set; }
    public string? ExtStr1 { get; set; }
    public string? ExtStr2 { get; set; }
}