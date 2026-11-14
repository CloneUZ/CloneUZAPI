using Microsoft.Extensions.Options;

namespace CloneUZ.Api.Services;


public sealed class ExpiredHoldSweeper(
    IServiceScopeFactory scopeFactory,
    IOptions<ExpiredHoldSweeperOptions> options,
    TimeProvider clock,
    ILogger<ExpiredHoldSweeper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval, clock);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var cleaner = scope.ServiceProvider.GetRequiredService<IExpiredHoldCleaner>();

                var released = await cleaner.CleanAsync(stoppingToken);
                if (released > 0)
                    logger.LogInformation("Released {Count} expired seat holds", released);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Expired hold sweep failed");
            }
        }
    }
}
