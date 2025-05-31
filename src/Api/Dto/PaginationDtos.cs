using System.ComponentModel.DataAnnotations;

namespace Api.Dto;

public class PaginationOptionsDto
{
    [Required]
    public string Cursor = string.Empty;

    [Range(0, 100)]
    public int PageSize = 50;
}

public class PaginatedResponse<T>
{
    public required IEnumerable<T> Items;
    public required string? NextCursor;
}
