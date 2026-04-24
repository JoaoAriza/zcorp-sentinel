using IdentityDefense.Application.Commands;
using IdentityDefense.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIdentityRiskCaseCommand command)
    {
        var result = await _handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _repository.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}