using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Model;

public class AuditedEntity : Entity
{
    [Column("created_audit_record_id")]
    public Guid CreatedAuditRecordId { get; set; }

    [Column("updated_audit_record_id")]
    public Guid UpdatedAuditRecordId { get; set; }

    [Column("deleted_audit_record_id")]
    public Guid? DeletedAuditRecordId { get; set; }
}
