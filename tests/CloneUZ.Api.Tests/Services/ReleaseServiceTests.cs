using CloneUZ.Api.Data;
using CloneUZ.Api.Services;
using Moq;

namespace CloneUZ.Api.Tests.Services;

public sealed class ReleaseServiceTests
{
    private static readonly Guid TripId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<ISeatRepository> _repo = new();
    private readonly ReleaseService _sut;

    public ReleaseServiceTests() => _sut = new ReleaseService(_repo.Object);

    [Fact]
    public async Task Passes_distinct_seats_and_returns_released_count()
    {
        _repo.Setup(r => r.ReleaseAsync(TripId, 2, It.Is<IReadOnlyCollection<int>>(s => s.SequenceEqual(new[] { 3, 1 })),
                UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var released = await _sut.ReleaseAsync(new ReleaseSeatsCommand(UserId, TripId, 2, [3, 1, 3]), CancellationToken.None);

        Assert.Equal(2, released);
    }

    [Fact]
    public async Task No_seats_means_release_all_users_holds_in_wagon()
    {
        _repo.Setup(r => r.ReleaseAsync(TripId, 2, null, UserId, It.IsAny<CancellationToken>())).ReturnsAsync(4);

        var released = await _sut.ReleaseAsync(new ReleaseSeatsCommand(UserId, TripId, 2, null), CancellationToken.None);

        Assert.Equal(4, released);
    }

    [Fact]
    public async Task Repeated_release_is_not_an_error()
    {
        _repo.Setup(r => r.ReleaseAsync(TripId, 2, It.IsAny<IReadOnlyCollection<int>?>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var released = await _sut.ReleaseAsync(new ReleaseSeatsCommand(UserId, TripId, 2, [1]), CancellationToken.None);

        Assert.Equal(0, released);
    }
}
