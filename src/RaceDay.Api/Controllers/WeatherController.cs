using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class WeatherController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public WeatherController(RaceDayDbContext db) => _db = db;

    /// <summary>Returns weather records associated with an event.</summary>
    [HttpGet("api/events/{eventId:int}/weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventWeather(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var weather = await _db.EventWeather
            .AsNoTracking()
            .Where(w => w.EventID == eventId)
            .OrderByDescending(w => w.WeatherDate)
            .ToListAsync();

        return Ok(weather);
    }

    /// <summary>Adds weather information to an event owned by the Organiser.</summary>
    [HttpPost("api/events/{eventId:int}/weather")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddEventWeather(int eventId, WeatherRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var eventEntity = await _db.Events.SingleOrDefaultAsync(e => e.EventID == eventId);

        if (eventEntity is null) return NotFound(new { message = "Event not found." });
        if (eventEntity.OrganiserID != organiserId) return Forbid();

        var weather = new EventWeather
        {
            EventID = eventId,
            WeatherDate = request.WeatherDate.Date,
            Temperature = request.Temperature,
            WeatherCondition = request.WeatherCondition?.Trim(),
            Humidity = request.Humidity
        };

        _db.EventWeather.Add(weather);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}/weather", weather);
    }
}
