namespace CloneUZ.Api.Domain;

/// <summary>
/// Место на схеме вагона. X/Y — координаты в сетке, по ним фронт рисует схему любого типа вагона.
/// </summary>
public sealed record SeatLayout(int Number, int Compartment, BerthLevel Berth, SeatFeatures Features, int X, int Y);

/// <summary>
/// Схемы вагонов. Схема зависит только от типа вагона, поэтому считается один раз
/// и хранится в памяти — в БД её класть не нужно.
/// </summary>
public static class WagonLayouts
{
    private static readonly Dictionary<WagonType, IReadOnlyList<SeatLayout>> Layouts = new()
    {
        [WagonType.Compartment] = BuildCompartments(seatsPerCompartment: 4),
        [WagonType.Lux] = BuildCompartments(seatsPerCompartment: 2),
        [WagonType.Platzkart] = BuildPlatzkart(),
    };

    public static IReadOnlyList<SeatLayout> For(WagonType type) => Layouts[type];

    // Купе и СВ: 9 купе. Нечётные места — нижние (в СВ все нижние). Крайние купе — у туалета.
    private static List<SeatLayout> BuildCompartments(int seatsPerCompartment)
    {
        var seats = new List<SeatLayout>();
        for (var n = 1; n <= 9 * seatsPerCompartment; n++)
        {
            var compartment = (n - 1) / seatsPerCompartment + 1;
            var indexInCompartment = (n - 1) % seatsPerCompartment;
            var berth = seatsPerCompartment == 2 || n % 2 == 1 ? BerthLevel.Lower : BerthLevel.Upper;

            var features = SeatFeatures.Window;
            if (berth == BerthLevel.Lower) features |= SeatFeatures.PowerOutlet;
            if (compartment is 1 or 9) features |= SeatFeatures.NearToilet;

            // СВ: два места рядом по X. Купе: нижнее и верхнее на одном X, разные Y.
            var x = (compartment - 1) * 2 + (seatsPerCompartment == 2 ? indexInCompartment : indexInCompartment / 2);
            var y = berth == BerthLevel.Lower ? 1 : 0;
            seats.Add(new SeatLayout(n, compartment, berth, features, x, y));
        }
        return seats;
    }

    // Плацкарт: 1–36 основные (9 отсеков по 4), 37–54 боковые (нечётные — нижние).
    private static List<SeatLayout> BuildPlatzkart()
    {
        var seats = BuildCompartments(seatsPerCompartment: 4);

        for (var n = 37; n <= 54; n++)
        {
            var compartment = 9 - (n - 37) / 2;
            var berth = n % 2 == 1 ? BerthLevel.Lower : BerthLevel.Upper;

            var features = SeatFeatures.Window | SeatFeatures.Side;
            if (compartment is 1 or 9) features |= SeatFeatures.NearToilet;

            var y = berth == BerthLevel.Lower ? 3 : 4;
            seats.Add(new SeatLayout(n, compartment, berth, features, (compartment - 1) * 2, y));
        }
        return seats;
    }
}
