using System.Text.Json.Serialization;

namespace MyApi.Application.Identities.Dto;

public class UserPcMenuDto
{
    public UserPcMenuMetaDto Meta { get; set; } = new();
    public string? Name { get; set; }
    public string? Path { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Redirect { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Component { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<UserPcMenuDto>? Children { get; set; }
    
    
    [JsonIgnore]
    public Guid Id { get; set; }
    [JsonIgnore]
    public int Level { get; set; }
    [JsonIgnore]
    public Guid? ParentId { get; set; }
}

public class UserPcMenuMetaDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Order { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AffixTab { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Icon { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? NoBasicLayout { get; set; } = false;
}
