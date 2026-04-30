using IdentityDefense.Application.DTOs.Audit;
using IdentityDefense.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IdentityDefenseDbContext _context;

    public AuditLogsController(IdentityDefenseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? action,
        [FromQuery] string? userEmail,
        [FromQuery] string? resource,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);

        if (!string.IsNullOrWhiteSpace(userEmail))
            query = query.Where(x => x.UserEmail != null && x.UserEmail.Contains(userEmail));

        if (!string.IsNullOrWhiteSpace(resource))
            query = query.Where(x => x.Resource == resource);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.Action,
                x.Resource,
                x.ResourceId,
                x.IpAddress,
                x.UserAgent,
                x.CreatedAt
            ))
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }
}