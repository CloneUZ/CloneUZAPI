namespace CloneUZ.Domain.Models;

public class TrainRun
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public Guid RouteId { get; set; }
    public DateOnly Date { get; set; }
    public int DelayMinuts { get; set; }
    public string Status { get; set; }
    
    public Train Train { get; set; }
    public Route Route { get; set; }
}