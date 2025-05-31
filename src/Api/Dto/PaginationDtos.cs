using System.ComponentModel.DataAnnotations;

namespace Api.Dto;

public class PaginationOptionsDto
{
    public string? Cursor { get; set; }

    [Required]
    [Range(1, 100)]
    public int PageSize { get; set; }
}

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public string? NextCursor { get; set; }
}
