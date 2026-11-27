using CloneUZ.BLL.Interfaces;
using DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.API.Controllers;

[ApiController]
[Route("find/tickets/[controller]")]
public class FindTicketsController:ControllerBase
{
    private readonly ITrainRunService  _service;
    public FindTicketsController(ITrainRunService service)
    {
        _service = service;
    }
    [HttpPost]
    public async Task<IActionResult> FindTickets( [FromBody] FindTicket request)
    {
        if (request == null) return BadRequest("bad info from client");
        return Ok( await _service.FindTickets(request));
    }
}