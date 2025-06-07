using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Api.Model;


[Table("users")]
public class User() : AuditedEntity(type: nameof(User))
{
    [Column("name")]
    [StringLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    [StringLength(150)]
    [Required]
    public string Email { get; set; } = string.Empty;

}
