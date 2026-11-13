using System.Security.Claims;
using CloneUZ.Api.Common;
using CloneUZ.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.Api.Controllers;

public sealed record HoldSeatsRequest(IReadOnlyList<int> Seats);

[ApiController]
[Route("api/trips/{tripId:guid}/wagons/{wagonNumber:int}/holds")]
public sealed class HoldsController(IHoldService holdService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<HoldDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Hold(Guid tripId, int wagonNumber, HoldSeatsRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await holdService.HoldAsync(new HoldSeatsCommand(userId, tripId, wagonNumber, request.Seats), ct);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : ToProblem(result.Error!);
    }
    
    private ObjectResult ToProblem(Error error)
    {
        var status = error.Kind switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status409Conflict,
        };
        var problem = Problem(statusCode: status, title: error.Code, detail: error.Message);
        if (error.Details is not null && problem.Value is ProblemDetails pd)
            foreach (var (key, value) in error.Details) pd.Extensions[key] = value;
        return problem;
    }
}
