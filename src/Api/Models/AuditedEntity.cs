using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Model;

public partial class AuditedEntity(string type) : Entity
{
    [NotMapped]
    public string Type { get; init; } = type;

    [Column("created_audit_record_id")]
    public Guid CreatedAuditRecordId { get; set; }

    [Column("updated_audit_record_id")]
    public Guid UpdatedAuditRecordId { get; set; }

    [Column("deleted_audit_record_id")]
    public Guid? DeletedAuditRecordId { get; set; }

    // Set to trigger a soft-deletion when saved.
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}
