namespace IdentityDefense.Application.DTOs.Audit;

public record AuditLogResponse(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string Resource,
    string? ResourceId,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);