using IdentityDefense.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDefense.API.Controllers;

[ApiController]
[Route("api/identity-risk-cases")]
public class IdentityRiskCasesController : ControllerBase
{
    private readonly CreateIdentityRiskCaseHandler _handler;

    public IdentityRiskCasesController(CreateIdentityRiskCaseHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIdentityRiskCaseCommand command)
    {
        var result = await _handler.Handle(command);
        return Ok(result);
    }
}