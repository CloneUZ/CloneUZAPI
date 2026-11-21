namespace CloneUZ.Api.Domain;

/// <summary>
/// Вагон в составе конкретного рейса: номер и тип.
/// Нужен, чтобы знать, какую схему показывать. Когда Дмитро сделает сущность рейса (Trip),
/// сюда добавится внешний ключ на неё.
/// </summary>
public sealed class TripWagon
{
    public Guid TripId { get; set; }
    public int Number { get; set; }
    public WagonType Type { get; set; }
}
