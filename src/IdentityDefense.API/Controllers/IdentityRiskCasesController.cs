using IdentityDefense.Application.Commands;
using IdentityDefense.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace IdentityDefense.API.Controllers;

[ApiController]
[Route("api/identity-risk-cases")]
public class IdentityRiskCasesController : ControllerBase
{
    private readonly CreateIdentityRiskCaseHandler _handler;
    private readonly IIdentityRiskCaseRepository _repository;

    public IdentityRiskCasesController(
        CreateIdentityRiskCaseHandler handler,
        IIdentityRiskCaseRepository repository)
    {
        _handler = handler;
        _repository = repository;
    }

    [Authorize(Roles = "Admin,Analyst")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIdentityRiskCaseCommand command)
    {
        var result = await _handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Analyst,Viewer")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _repository.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Analyst,Viewer")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}