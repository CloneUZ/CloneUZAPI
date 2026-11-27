using System.ComponentModel.DataAnnotations;

namespace CloneUZ.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Route
{
    public Guid Id {  get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    public List<RouteStation> Sations { get; set; }
        
    public List<TrainRun> TrainRun { get; set; }
}