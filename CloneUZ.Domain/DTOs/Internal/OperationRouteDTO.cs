using CloneUZ.Domain.Models;

namespace CloneUZ.Domain.DTOs;

public class OperationRouteDTO
{
    public List<Route> RoutesOrigin { get; set; }
    public List<RouteStationDTO>? StationsTransfer { get; set; }

    public List<Route> RoutesClientOrder { get; set; }
    
    public string? RouteTag { get; set; }

    public TimeSpan FullDuration { get; set; }
    public int DaysInRun { get; set; }
    
    public TimeOnly StartedAt { get; set; }
    public TimeOnly EndedAt { get; set; }
}