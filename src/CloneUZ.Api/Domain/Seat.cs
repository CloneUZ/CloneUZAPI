namespace CloneUZ.Api.Domain;

/// <summary>
/// Место в вагоне на конкретном рейсе. Одна строка в БД = одно место.
/// Статус меняется только через методы Reserve / CancelReservation / Sell — так правила нельзя обойти.
/// </summary>
public sealed class Seat
{
    private Seat() { } // нужен EF Core

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

    /// <summary>
    /// Версия строки (в PostgreSQL — системная колонка xmin). Если два пользователя одновременно
    /// меняют одно место, второй получит DbUpdateConcurrencyException. Подключим на шаге 3.
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>Просроченный резерв считается свободным, даже если фоновая очистка ещё не отработала.</summary>
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

    /// <summary>Отменить может только тот, кто резервировал.</summary>
    public void CancelReservation(Guid userId)
    {
        if (Status != SeatStatus.Reserved || ReservedBy != userId)
            throw new InvalidOperationException($"Seat {SeatNumber} is not reserved by this user.");

        Release();
    }

    /// <summary>Продажа — только по своему и ещё действующему резерву.</summary>
    public void Sell(Guid userId, DateTimeOffset now)
    {
        if (Status != SeatStatus.Reserved || ReservedBy != userId)
            throw new InvalidOperationException($"Seat {SeatNumber} is not reserved by this user.");
        if (ReservedUntil <= now)
            throw new InvalidOperationException($"Reservation for seat {SeatNumber} has expired.");

        Status = SeatStatus.Sold;
        ReservedUntil = null;
    }

    /// <summary>Для фоновой очистки просроченных резервов (шаг 7).</summary>
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
