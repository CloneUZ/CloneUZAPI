using CloneUZ.Api.Domain;
using Microsoft.AspNetCore.SignalR;

namespace CloneUZ.Api.Realtime;


public sealed class SeatMapHub : Hub
{
    public const string Path = "/hubs/seat-map";

    public Task JoinWagon(Guid tripId, int wagonNumber) =>
        Groups.AddToGroupAsync(Context.ConnectionId, SeatMapNotifier.Group(tripId, wagonNumber));

    public Task LeaveWagon(Guid tripId, int wagonNumber) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, SeatMapNotifier.Group(tripId, wagonNumber));
}

public sealed record SeatsChangedEvent(Guid TripId, int WagonNumber, IReadOnlyList<int>? Seats, SeatStatus Status);

public interface ISeatMapNotifier
{
    Task SeatsChangedAsync(Guid tripId, int wagonNumber, IReadOnlyList<int>? seats, SeatStatus status);
    Task SeatMapsChangedAsync();
}

public sealed class SeatMapNotifier(IHubContext<SeatMapHub> hub, ILogger<SeatMapNotifier> logger) : ISeatMapNotifier
{
    public const string SeatsChanged = "SeatsChanged";
    public const string SeatMapsChanged = "SeatMapsChanged";

    public static string Group(Guid tripId, int wagonNumber) => $"{tripId:N}:{wagonNumber}";

    public async Task SeatsChangedAsync(Guid tripId, int wagonNumber, IReadOnlyList<int>? seats, SeatStatus status)
    {
        try
        {
            await hub.Clients.Group(Group(tripId, wagonNumber))
                .SendAsync(SeatsChanged, new SeatsChangedEvent(tripId, wagonNumber, seats, status));
        }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to push seat map update"); }
    }

    public async Task SeatMapsChangedAsync()
    {
        try { await hub.Clients.All.SendAsync(SeatMapsChanged); }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to push seat map refresh"); }
    }
}
