using System.ComponentModel.DataAnnotations;

namespace Cits.Dtos;

public class PagedRequestDto
{
    public static int DefaultMaxResultCount { get; set; } = 20;


    [Range(0, int.MaxValue)] public int SkipCount { get; set; }

    [Range(1, 20000000)] public int MaxResultCount { get; set; } = DefaultMaxResultCount;
}