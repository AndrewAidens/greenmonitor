using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using greenmonitor.Data;
using greenmonitor.Models;

namespace greenmonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SensorController(AppDbContext db)
        {
            _db = db;
        }

        // Этот эндпоинт для Arduino — без JWT, но с API-ключом
        [HttpPost("readings")]
        public async Task<IActionResult> AddReading(
            [FromHeader(Name = "X-Api-Key")] string apiKey,
            [FromBody] SensorReading reading)
        {
            if (apiKey != "arduino-secret-key")
                return Unauthorized("Wrong API key!");

            reading.Timestamp = DateTime.UtcNow;
            _db.SensorReadings.Add(reading);
            await _db.SaveChangesAsync();

            return Ok("Data saved!");
        }

        // Все эндпоинты ниже требуют JWT
        [Authorize]
        [HttpGet("readings")]
        public async Task<IActionResult> GetReadings(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _db.SensorReadings.AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(r => r.Timestamp <= to.Value);

            var readings = await query
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            return Ok(readings);
        }

        [Authorize]
        [HttpGet("readings/latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _db.SensorReadings
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();

            if (latest == null)
                return NotFound("Data doesn't exist yet!");

            return Ok(latest);
        }
    }
}