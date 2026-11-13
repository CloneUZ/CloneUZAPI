using System.Security.Claims;
using CloneUZ.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.Api.Controllers;

[ApiController]
[Route("api/trips/{tripId:guid}/wagons/{wagonNumber:int}")]
public sealed class SeatMapController(ISeatMapService seatMapService) : ControllerBase
{
    /// <summary>Интерактивная схема вагона: расположение мест, их характеристики и статусы.</summary>
    [HttpGet("seat-map")]
    [ProducesResponseType<SeatMapDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatMapDto>> GetSeatMap(Guid tripId, int wagonNumber, CancellationToken ct)
    {
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;

        var seatMap = await seatMapService.GetSeatMapAsync(tripId, wagonNumber, userId, ct);
        return seatMap is null ? NotFound() : Ok(seatMap);
    }
}


//
// GET /api/trips/11111111-1111-1111-1111-111111111111/wagons/2/seat-map
//
// {
//     "tripId": "11111111-1111-1111-1111-111111111111",
//     "wagonNumber": 2,
//     "wagonType": "Compartment",
//     "availableCount": 34,
//     "seats": [
//     {
//         "number": 1,
//         "compartment": 1,
//         "berth": "Lower",
//         "features": 7,
//         "x": 0,
//         "y": 1,
//         "status": "Available",
//         "isMine": false,
//         "reservedUntil": null
//     },
//     {
//         "number": 2,
//         "compartment": 1,
//         "berth": "Upper",
//         "features": 5,
//         "x": 0,
//         "y": 0,
//         "status": "Reserved",
//         "isMine": false,
//         "reservedUntil": null
//     },
//     {
//         "number": 3,
//         "compartment": 1,
//         "berth": "Lower",
//         "features": 7,
//         "x": 1,
//         "y": 1,
//         "status": "Available",
//         "isMine": false,
//         "reservedUntil": null
//     },
//     {
//         "number": 4,
//         "compartment": 1,
//         "berth": "Upper",
//         "features": 5,
//         "x": 1,
//         "y": 0,
//         "status": "Available",
//         "isMine": false,
//         "reservedUntil": null
//     }
//     ]
// }