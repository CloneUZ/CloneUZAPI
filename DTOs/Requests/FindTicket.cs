namespace DTOs.Requests;

public class FindTicket
{
    public int From { get; set; }
    public int To { get; set; }
    
    public DateOnly Date { get; set; }
    
    public int TransfersCount { get; set; }
    
    public List<string>? TimeStartLimits { get; set; }
    public uint? FullRouteTimeLimit { get; set; }
    
    public List<string>? TrainsType { get; set; }
    
    public uint PriceFrom { get; set; }
    public uint PriceTo { get; set; }
}