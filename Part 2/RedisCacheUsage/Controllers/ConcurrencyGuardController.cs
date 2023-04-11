using Microsoft.AspNetCore.Mvc;
using RedisUsage.ConcurrencyGuard;

namespace RedisCacheUsage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConcurrencyGuardController : ControllerBase
    {
        private readonly ConcurrencyGuard _concurrencyGuard;

        public ConcurrencyGuardController(ConcurrencyGuard concurrencyGuard)
        {
            _concurrencyGuard = concurrencyGuard;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string eventName)
        {
            var result = await _concurrencyGuard.AddEvent(eventName);
            if (result)
            {
                return Ok("The event was added.");
            }
            return BadRequest("The event already exists.");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string eventName)
        {
            var result = await _concurrencyGuard.RemoveEvent(eventName);
            if (result)
            {
                return Ok("The event was removed.");
            }
            return BadRequest("The event does not exist.");
        }
    }
}
