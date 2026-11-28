using StackExchange.Redis;

namespace CloneUZ.Api.Services;


public interface ISeatLockService
{
    Task<int[]> TryAcquireAsync(Guid tripId, int wagonNumber, IReadOnlyList<int> seats, Guid userId, TimeSpan ttl);
    
    Task ReleaseAsync(Guid tripId, int wagonNumber, IReadOnlyList<int> seats, Guid userId);
}

public sealed class RedisSeatLockService(IConnectionMultiplexer redis) : ISeatLockService
{
    private const string AcquireScript = """
        local conflicts = {}
        for i, key in ipairs(KEYS) do
            local owner = redis.call('GET', key)
            if owner and owner ~= ARGV[1] then
                table.insert(conflicts, i)
            end
        end
        if #conflicts > 0 then
            return conflicts
        end
        for _, key in ipairs(KEYS) do
            redis.call('SET', key, ARGV[1], 'PX', ARGV[2])
        end
        return {}
        """;

    private const string ReleaseScript = """
        local released = 0
        for _, key in ipairs(KEYS) do
            if redis.call('GET', key) == ARGV[1] then
                redis.call('DEL', key)
                released = released + 1
            end
        end
        return released
        """;

    public async Task<int[]> TryAcquireAsync(Guid tripId, int wagonNumber, IReadOnlyList<int> seats, Guid userId, TimeSpan ttl)
    {
        var result = await redis.GetDatabase().ScriptEvaluateAsync(
            AcquireScript, Keys(tripId, wagonNumber, seats), [userId.ToString("N"), (long)ttl.TotalMilliseconds]);
        
        return ((RedisResult[])result!).Select(r => seats[(int)r - 1]).ToArray();
    }

    public Task ReleaseAsync(Guid tripId, int wagonNumber, IReadOnlyList<int> seats, Guid userId) =>
        redis.GetDatabase().ScriptEvaluateAsync(ReleaseScript, Keys(tripId, wagonNumber, seats), [userId.ToString("N")]);
    
    private static RedisKey[] Keys(Guid tripId, int wagonNumber, IReadOnlyList<int> seats) =>
        seats.Select(s => (RedisKey)$"seat-lock:{{{tripId:N}:{wagonNumber}}}:{s}").ToArray();
}
