using CloneUZ.Api.Data;
using CloneUZ.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace CloneUZ.Api.Tests.Services;

public sealed class ExpiredHoldCleanerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ISeatRepository> _repo = new();

    private ExpiredHoldCleaner CreateSut(int batchSize = 100, int maxBatches = 20) =>
        new(_repo.Object,
            Options.Create(new ExpiredHoldSweeperOptions { BatchSize = batchSize, MaxBatchesPerRun = maxBatches }),
            new FakeTimeProvider(Now));

    [Fact]
    public async Task Keeps_going_while_batches_are_full_and_stops_on_partial()
    {
        _repo.SetupSequence(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100).ReturnsAsync(100).ReturnsAsync(37);

        var total = await CreateSut().CleanAsync(CancellationToken.None);

        Assert.Equal(237, total);
        _repo.Verify(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Nothing_expired_means_single_query()
    {
        _repo.Setup(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var total = await CreateSut().CleanAsync(CancellationToken.None);

        Assert.Equal(0, total);
        _repo.Verify(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Stops_at_max_batches_per_run()
    {
        _repo.Setup(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>())).ReturnsAsync(100);

        var total = await CreateSut(maxBatches: 5).CleanAsync(CancellationToken.None);

        Assert.Equal(500, total);
        _repo.Verify(r => r.ReleaseExpiredBatchAsync(Now, 100, It.IsAny<CancellationToken>()), Times.Exactly(5));
    }
}
