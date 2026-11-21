using System.ComponentModel.DataAnnotations;

namespace CloneUZ.Domain.Models;

public class Train
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    [MaxLength(255)]
    public string Type { get; set; }
    public double AvgSpeed { get; set; }
    public double MoneyPerMeter { get; set; }
    
    public List<TrainRun> TrainsRun { get; set; }
}