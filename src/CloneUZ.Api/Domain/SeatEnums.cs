namespace CloneUZ.Api.Domain;

public enum SeatStatus
{
    Available = 0,
    Reserved = 1,
    Sold = 2,
}

public enum WagonType
{
    Compartment = 1,
    Platzkart = 2,
    Lux = 3,
}

public enum BerthLevel
{
    Lower = 1,
    Upper = 2,
}

[Flags]
public enum SeatFeatures
{
    None = 0, 
    Window = 1,
    PowerOutlet = 2,
    NearToilet = 4,
    Side = 8,
}
