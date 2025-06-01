namespace Api.Dto;

public class AuditRecordDto
{
    public required Guid Id { get; set; }
    public required DateTime TimeStamp { get; set; }
    public required string TargetType { get; set; }
    public required Guid TargetId { get; set; }
    public required string Action { get; set; }
}


public class ListAuditRecordsRequest
{
    public PaginationOptionsDto? PaginationOptions { get; set; }
}

