using System.Security.Claims;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDefense.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IAuditService _auditService;

    public UsersController(IUserRepository users, IAuditService auditService)
    {
        _users = users;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _users.GetAllAsync();

        return Ok(users.Select(x => new
        {
            x.Id,
            x.Name,
            x.Email,
            x.Role,
            x.CreatedAt
        }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _users.GetByIdAsync(id);

        if (user is null)
            return NotFound(new { message = "User not found." });

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == user.Id.ToString())
            return BadRequest(new { message = "You cannot delete your own admin account." });

        await _users.DeleteAsync(user);

        await _auditService.LogAsync(new AuditLog(
            Guid.TryParse(currentUserId, out var parsedUserId) ? parsedUserId : null,
            User.FindFirst(ClaimTypes.Email)?.Value,
            "USER_DELETED",
            "User",
            user.Id.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return NoContent();
    }
}