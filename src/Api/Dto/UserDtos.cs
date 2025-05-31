using System.ComponentModel.DataAnnotations;

namespace Api.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}
