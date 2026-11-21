namespace CloneUZ.Domain.DTOs;

public class RouteStationDTO
{
    public int StationOrder { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public TimeOnly DepartTime { get; set; }
    public StationDTO Station { get; set; }
}