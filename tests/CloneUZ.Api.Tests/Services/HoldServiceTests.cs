using Microsoft.Extensions.Logging.Abstractions;
using CloneUZ.Api.Common;
using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;
using CloneUZ.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace CloneUZ.Api.Tests.Services;

public sealed class HoldServiceTests
{
    private static readonly Guid TripId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ISeatRepository> _repo = new();
    private readonly Mock<ISeatLockService> _locks = new() { DefaultValue = DefaultValue.Empty };
    private readonly HoldService _sut;

    public HoldServiceTests()
    {
        _repo.Setup(r => r.GetWagonTypeAsync(TripId, 2, It.IsAny<CancellationToken>())).ReturnsAsync(WagonType.Compartment);
        _sut = new HoldService(_repo.Object, _locks.Object, Options.Create(new SeatHoldOptions()), new FakeTimeProvider(Now), NullLogger<HoldService>.Instance);
    }

    private Task<Result<HoldDto>> Hold(params int[] seats) =>
        _sut.HoldAsync(new HoldSeatsCommand(UserId, TripId, 2, seats), CancellationToken.None);

    private void RepoReserves(bool success) =>
        _repo.Setup(r => r.TryReserveAsync(TripId, 2, It.IsAny<IReadOnlyCollection<int>>(), UserId, Now,
                It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);

    [Fact]
    public async Task Success_returns_sorted_distinct_seats_and_ttl()
    {
        RepoReserves(true);

        var result = await Hold(3, 1, 3);

        Assert.True(result.IsSuccess);
        Assert.Equal([1, 3], result.Value.Seats);
        Assert.Equal(Now.AddMinutes(15), result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Redis_conflict_returns_409_without_touching_database()
    {
        _locks.Setup(l => l.TryAcquireAsync(TripId, 2, It.IsAny<IReadOnlyList<int>>(), UserId, It.IsAny<TimeSpan>()))
            .ReturnsAsync([3]);

        var result = await Hold(1, 3);

        Assert.Equal(ErrorKind.Conflict, result.Error!.Kind);
        _repo.Verify(r => r.TryReserveAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Database_conflict_rolls_back_redis_lock()
    {
        RepoReserves(false);

        await Hold(1);

        _locks.Verify(l => l.ReleaseAsync(TripId, 2, It.Is<IReadOnlyList<int>>(s => s.SequenceEqual(new[] { 1 })), UserId), Times.Once);
    }

    [Fact]
    public async Task Redis_down_falls_back_to_database()
    {
        _locks.Setup(l => l.TryAcquireAsync(TripId, 2, It.IsAny<IReadOnlyList<int>>(), UserId, It.IsAny<TimeSpan>()))
            .ThrowsAsync(new TimeoutException("redis down"));
        RepoReserves(true);

        var result = await Hold(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Taken_seat_returns_conflict()
    {
        RepoReserves(false);

        var result = await Hold(1);

        Assert.Equal(ErrorKind.Conflict, result.Error!.Kind);
    }

    [Fact]
    public async Task Empty_selection_is_validation_error_and_db_untouched()
    {
        var result = await Hold();

        Assert.Equal("seats_required", result.Error!.Code);
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Too_many_seats_is_rejected()
    {
        var result = await Hold(1, 2, 3, 4, 5);

        Assert.Equal("too_many_seats", result.Error!.Code);
    }

    [Fact]
    public async Task Unknown_wagon_returns_not_found()
    {
        var result = await _sut.HoldAsync(new HoldSeatsCommand(UserId, TripId, 99, [1]), CancellationToken.None);

        Assert.Equal(ErrorKind.NotFound, result.Error!.Kind);
    }

    [Fact]
    public async Task Seat_outside_layout_is_rejected_without_reserving()
    {
        var result = await Hold(1, 999);

        Assert.Equal("invalid_seat_number", result.Error!.Code);
        _repo.Verify(r => r.TryReserveAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
