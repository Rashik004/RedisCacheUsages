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
        using var redLock = await _redLockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(1));
        if(redLock.IsAcquired==false)
        {
            System.Console.WriteLine("Could not acquire lock");
            return false;
        }
        if(await _distributedCache.GetAsync(key) == null)
        {
            await _distributedCache.SetAsync(key, value);
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
