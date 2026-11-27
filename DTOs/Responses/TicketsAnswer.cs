using CloneUZ.Domain.DTOs;

namespace DTOs.Responses;

public class TicketsAnswer
{
    public List<Guid> RoutesIds { get; set; }
    public Guid TrainId { get; set; }
    
    public List<RouteDTO> RoutesOrigin { get; set; }
    public List<RouteStationDTO>? StationsTransfer { get; set; }
    public List<RouteDTO> RoutesClientOrder { get; set; }
    
    public int Number { get; set; }
    public string Type  { get; set; } 
    
    public string StationFromName { get; set; }
    public string StationToName { get; set; }
    
    public DateOnly StartedDate { get; set; }
    public DateOnly EndedDate { get; set; }
    
    public TimeOnly StartedTime { get; set; }
    public TimeOnly EndedTime { get; set; }
    
    public TimeSpan FullRouteTime  { get; set; }
    public double Price { get; set; }
}