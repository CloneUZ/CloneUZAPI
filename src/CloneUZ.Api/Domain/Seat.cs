namespace CloneUZ.Api.Domain;

/// <summary>
/// Место в вагоне на конкретном рейсе. Одна строка в БД = одно место.
/// Статус меняется только через методы Reserve / CancelReservation / Sell — так правила нельзя обойти.
/// </summary>
public sealed class Seat
{
    private Seat() { }

    public Seat(Guid tripId, int wagonNumber, int seatNumber)
    {
        TripId = tripId;
        WagonNumber = wagonNumber;
        SeatNumber = seatNumber;
        Status = SeatStatus.Available;
    }

    public int Id { get; private set; }
    public Guid TripId { get; private set; }
    public int WagonNumber { get; private set; }
    public int SeatNumber { get; private set; }
    public SeatStatus Status { get; private set; }
    public Guid? ReservedBy { get; private set; }
    public DateTimeOffset? ReservedUntil { get; private set; }

    public uint Version { get; private set; }

    public bool IsAvailable(DateTimeOffset now) =>
        Status == SeatStatus.Available || (Status == SeatStatus.Reserved && ReservedUntil <= now);

    public void Reserve(Guid userId, DateTimeOffset now, TimeSpan duration)
    {
        if (!IsAvailable(now))
            throw new InvalidOperationException($"Seat {SeatNumber} in wagon {WagonNumber} is not available.");

        Status = SeatStatus.Reserved;
        ReservedBy = userId;
        ReservedUntil = now + duration;
    }

    public void CancelReservation(Guid userId)
    {
        if (Status != SeatStatus.Reserved || ReservedBy != userId)
            throw new InvalidOperationException($"Seat {SeatNumber} is not reserved by this user.");

        Release();
    }

    public void Sell(Guid userId, DateTimeOffset now)
    {
        if (Status != SeatStatus.Reserved || ReservedBy != userId)
            throw new InvalidOperationException($"Seat {SeatNumber} is not reserved by this user.");
        if (ReservedUntil <= now)
            throw new InvalidOperationException($"Reservation for seat {SeatNumber} has expired.");

        Status = SeatStatus.Sold;
        ReservedUntil = null;
    }

    public bool ReleaseIfExpired(DateTimeOffset now)
    {
        if (Status != SeatStatus.Reserved || ReservedUntil > now) return false;
        Release();
        return true;
    }

    private void Release()
    {
        Status = SeatStatus.Available;
        ReservedBy = null;
        ReservedUntil = null;
    }
}
