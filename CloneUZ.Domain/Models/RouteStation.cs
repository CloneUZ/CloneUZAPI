using System.ComponentModel.DataAnnotations.Schema;

namespace CloneUZ.Domain.Models;

public class RouteStation
{
    public Guid Id { get; set; }
    public int StationCode { get; set; }
    public int StationOrder { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public TimeOnly DepartTime { get; set; }
    public Guid RoutesId { get; set; }
    public Route Routes { get; set; }
    [ForeignKey(nameof(StationCode))]
    public Station Station { get; set; }
}