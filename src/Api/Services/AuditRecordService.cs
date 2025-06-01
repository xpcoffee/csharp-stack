using Api.Data;
using Api.Dto;
using Microsoft.EntityFrameworkCore;

namespace Api.Service;


public class AuditRecordService
{
    private readonly ApplicationDbContext _context;

    public AuditRecordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditRecord?> GetUserById(Guid auditRecordId, CancellationToken cancellationToken)
    {
        return await _context.AuditRecords.FindAsync(auditRecordId);
    }

    public async Task<List<AuditRecord>> ListAuditRecordsForTarget(
        Guid targetId,
        DateTime? dateTimeFrom,
        int pageSize,
        CancellationToken cancellationToken
        )
    {
        var query = _context.AuditRecords.AsQueryable()
          .Where(r => r.TargetId == targetId);

        if (dateTimeFrom.HasValue)
        {
            query = query.Where(r => r.TimeStamp < dateTimeFrom.Value);
        }

        return await query
          .OrderBy(r => r.TimeStamp)
          .Take(pageSize)
          .ToListAsync(cancellationToken);
    }
}


public static class AuditRecordeServiceLayerExtensions
{
    public static AuditRecordDto ToDto(this AuditRecord auditRecord)
    {
        return new AuditRecordDto()
        {
            Id = auditRecord.Id,
            TimeStamp = auditRecord.TimeStamp,
            Action = auditRecord.Action,
            TargetId = auditRecord.TargetId,
            TargetType = auditRecord.TargetType
        };
    }
}
