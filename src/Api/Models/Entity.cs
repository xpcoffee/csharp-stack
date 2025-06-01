namespace Api.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Entity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
}
