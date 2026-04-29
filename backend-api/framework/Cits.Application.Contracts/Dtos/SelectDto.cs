namespace Cits.Dtos;

public class GuidSelectDto
{
    public Guid Value { get; set; }

    public string Label { get; set; } = string.Empty;
}

public class SelectDto
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
