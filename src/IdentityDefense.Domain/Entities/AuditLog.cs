namespace IdentityDefense.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string? ResourceId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid? userId,
        string? userEmail,
        string action,
        string resource,
        string? resourceId,
        string? ipAddress,
        string? userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserEmail = userEmail;
        Action = action;
        Resource = resource;
        ResourceId = resourceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Action))
            throw new ArgumentException("Action is required.");

        if (string.IsNullOrWhiteSpace(Resource))
            throw new ArgumentException("Resource is required.");
    }
}