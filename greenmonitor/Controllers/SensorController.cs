using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using greenmonitor.Data;
using greenmonitor.Models;

namespace greenmonitor.Controllers
{
    /// <summary>
    /// Handles sensor data collection and retrieval.
    /// Data submission uses API key authentication (for Arduino),
    /// while data retrieval requires a valid JWT token.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SensorController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Receives sensor readings from Arduino.
        /// Authenticated via X-Api-Key header instead of JWT.
        /// </summary>
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


        /// <summary>
        /// Returns up to 50 most recent sensor readings.
        /// Supports optional date range filtering via query parameters.
        /// </summary>
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
                .Take(50)
                .ToListAsync();

            return Ok(readings);
        }

        /// <summary>
        /// Returns the most recent sensor reading.
        /// Used by the mobile app to display current greenhouse conditions.
        /// </summary>
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