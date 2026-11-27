using System.ComponentModel.DataAnnotations;

namespace CloneUZ.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Station
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Code { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(255)]
    public string City { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<RouteStation> RouteStations { get; set; }
}