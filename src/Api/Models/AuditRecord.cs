using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Model;

[Table("audit_records")]
public class AuditRecord : Entity
{
    [Column("timestamp")]
    [Required]
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    [Column("target_type")]
    [StringLength(100)]
    [Required]
    public required string TargetType { get; set; }

    [Column("target-id")]
    [Required]
    public required Guid TargetId { get; set; }

    [Column("action")]
    [StringLength(20)]
    [Required]
    public required string Action { get; set; }

    public static AuditRecord GetCreateAuditRecord(AuditedEntity item) => new AuditRecord
    {
        TargetId = item.Id,
        TargetType = item.Type,
        Action = AuditRecordAction.Create
    };
    public static AuditRecord GetUpdateAuditRecord(AuditedEntity item) => new AuditRecord
    {
        TargetId = item.Id,
        TargetType = item.Type,
        Action = AuditRecordAction.Update
    };
    public static AuditRecord GetAccessAuditRecord(AuditedEntity item) => new AuditRecord
    {
        TargetId = item.Id,
        TargetType = item.Type,
        Action = AuditRecordAction.Access
    };
    public static AuditRecord GetDeleteAuditRecord(AuditedEntity item) => new AuditRecord
    {
        TargetId = item.Id,
        TargetType = item.Type,
        Action = AuditRecordAction.Delete
    };
    public static AuditRecord GetDropAuditRecord(AuditedEntity item) => new AuditRecord
    {
        TargetId = item.Id,
        TargetType = item.Type,
        Action = AuditRecordAction.Drop
    };
}


public static class AuditRecordAction
{
    public static readonly string Create = "create";
    public static readonly string Update = "update";
    public static readonly string Access = "access";
    public static readonly string Delete = "delete"; // soft delete
    public static readonly string Drop = "drop"; // hard delete
}
