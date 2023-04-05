using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisCacheUsage.Controllers;
[ApiController]
[Route("[controller]")]
public class CrudController : ControllerBase
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CrudController(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    [HttpPost]
    public async Task<IActionResult> Post(string key, string value)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, value);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await EnsureKeyExists(key, db);
        var value = await db.StringGetAsync(key);
        var test=value.ToString();
        return Ok(test);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put(string key, string value)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await EnsureKeyExists(key, db);

        await db.StringSetAsync(key, value);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await EnsureKeyExists(key, db);
        
        await db.KeyDeleteAsync(key);
        return Ok();
    }

    private async Task EnsureKeyExists(string key, IDatabase db)
    {
        var result = await db.StringGetAsync(key);
        if(result.IsNull)
        {
            throw new Exception("The key does not exist.");
        }
    }
}