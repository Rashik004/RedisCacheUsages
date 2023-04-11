using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisUsage.ConcurrencyGuard;
public class ConcurrencyGuard
{
    private readonly IDistributedCache _distributedCache;
    private readonly RedLockFactory _redLockFactory;

    public ConcurrencyGuard(IDistributedCache distributedCache, IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        var redisConnectionString = configuration.GetSection("RedisConnectionString");
        var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString.Value);
        _redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> { multiplexer });
    }

    public async Task<bool> AddEvent(string eventName)
    {
        var key = GetEventName(eventName);
        var value = GetDefaultByteARray();
        var dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        using var redLock = await _redLockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(2));
        if (redLock.IsAcquired == false)
        {
            System.Console.WriteLine(
                $"{DateTime.UtcNow.ToString(dateTimeFormat)} Could not acquire lockfor {eventName}");
            return false;
        }
        System.Console.WriteLine(
                $"{DateTime.UtcNow.ToString(dateTimeFormat)} Acquired lock for {eventName}");
        if (await _distributedCache.GetAsync(key) == null)
        {
            System.Console.WriteLine(
                $"{DateTime.UtcNow.ToString(dateTimeFormat)} No duplicate event found. Adding Event: {eventName}");

            await _distributedCache.SetAsync(key, value);
            System.Console.WriteLine(
                $"{DateTime.UtcNow.ToString(dateTimeFormat)} Added Event: {eventName}");

            return true;
        }
        return false;
    }

    public async Task<bool> RemoveEvent(string eventName)
    {
        var key = GetEventName(eventName);
        if (await _distributedCache.GetAsync(key) != null)
        {
            await _distributedCache.RemoveAsync(key);
            return true;
        }
        return false;
    }

    private string GetEventName(string eventName)
    {
        return $"event:{eventName}";
    }
    private byte[] GetDefaultByteARray()
    {
        return Encoding.UTF8.GetBytes("1");
    }
}
