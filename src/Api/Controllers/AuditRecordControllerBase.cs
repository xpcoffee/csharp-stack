using Api.Dto;
using Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class AuditRecordControllerBase(AuditRecordService auditRecordService) : ControllerBase
{
    [HttpGet("{targetId}/audit-records")]
    public async Task<ActionResult<PaginatedResponse<AuditRecordDto>>> ListUsers(Guid targetId, ListAuditRecordsRequest? request)
    {
        DateTime? cursor = null;
        if (request?.PaginationOptions?.Cursor is not null)
        {
            if (DateTime.TryParse(request.PaginationOptions.Cursor, out var c))
            {
                cursor = c;
            }
            else
            {
                return BadRequest("The given cursor is not valid.");
            }

        }

        var cancellationToken = CancellationToken.None;
        var pageSize = request?.PaginationOptions?.PageSize ?? 50;
        var auditRecords = await auditRecordService.ListAuditRecordsForTarget(targetId, cursor, pageSize, cancellationToken);

        var hasMoreResults = auditRecords.Count == pageSize;

        var page = new PaginatedResponse<AuditRecordDto>()
        {
            Items = auditRecords.Select(u => u.ToDto()),
            NextCursor = hasMoreResults ? auditRecords.Last().Id.ToString() : null
        };

        return page;
    }
}
