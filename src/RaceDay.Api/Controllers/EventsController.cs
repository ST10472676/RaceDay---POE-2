using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    /// <summary>Returns all RaceDay events.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _db.Events
            .AsNoTracking()
            .Include(e => e.Route)
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventID, e.EventName, e.Description, e.EventDate, e.Location,
                e.Distance, e.EventType, e.BannerImageUrl, e.OrganiserID,
                Route = new { e.Route.RouteID, e.Route.StartLocation, e.Route.EndLocation, e.Route.RouteDirection }
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>Returns detailed information for one event.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(int id)
    {
        var item = await _db.Events
            .AsNoTracking()
            .Include(e => e.Route)
            .Include(e => e.Categories)
            .Include(e => e.WeatherRecords)
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID, e.EventName, e.Description, e.EventDate, e.Location,
                e.Distance, e.EventType, e.BannerImageUrl, e.OrganiserID,
                Route = new { e.Route.RouteID, e.Route.StartLocation, e.Route.EndLocation, e.Route.RouteDirection, e.Route.Distance },
                Categories = e.Categories.Select(c => new { c.CategoryID, c.CategoryName, c.MinAge, c.MaxAge }),
                Weather = e.WeatherRecords.Select(w => new { w.WeatherID, w.WeatherDate, w.Temperature, w.WeatherCondition, w.Humidity })
            })
            .SingleOrDefaultAsync();

        return item is null
            ? NotFound(new { message = "Event not found." })
            : Ok(item);
    }

    /// <summary>Creates an event owned by the currently authenticated Organiser.</summary>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (!IsValidEventType(request.EventType))
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        if (!await _db.Routes.AnyAsync(r => r.RouteID == request.RouteID))
            return BadRequest(new { message = "The selected route does not exist." });

        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var entity = new Event
        {
            OrganiserID = organiserId,
            RouteID = request.RouteID,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = NormaliseEventType(request.EventType),
            BannerImageUrl = request.BannerImageUrl?.Trim()
        };

        _db.Events.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEventById), new { id = entity.EventID }, entity);
    }

    /// <summary>Updates an event belonging to the currently authenticated Organiser.</summary>
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var entity = await _db.Events.SingleOrDefaultAsync(e => e.EventID == id);
        if (entity is null) return NotFound(new { message = "Event not found." });
        if (entity.OrganiserID != organiserId)
            return Forbid();

        if (!IsValidEventType(request.EventType))
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        if (!await _db.Routes.AnyAsync(r => r.RouteID == request.RouteID))
            return BadRequest(new { message = "The selected route does not exist." });

        entity.RouteID = request.RouteID;
        entity.EventName = request.EventName.Trim();
        entity.Description = request.Description?.Trim();
        entity.EventDate = request.EventDate.Date;
        entity.Location = request.Location.Trim();
        entity.Distance = request.Distance;
        entity.EventType = NormaliseEventType(request.EventType);
        entity.BannerImageUrl = request.BannerImageUrl?.Trim();

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    /// <summary>Deletes an event owned by the current Organiser and its dependent records.</summary>
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var entity = await _db.Events
            .Include(e => e.Categories)
            .Include(e => e.Enrolments).ThenInclude(e => e.Result)
            .Include(e => e.WeatherRecords)
            .SingleOrDefaultAsync(e => e.EventID == id);

        if (entity is null) return NotFound(new { message = "Event not found." });
        if (entity.OrganiserID != organiserId) return Forbid();

        var results = entity.Enrolments
            .Where(x => x.Result is not null)
            .Select(x => x.Result!)
            .ToList();

        _db.Results.RemoveRange(results);
        _db.Enrolments.RemoveRange(entity.Enrolments);
        _db.Categories.RemoveRange(entity.Categories);
        _db.EventWeather.RemoveRange(entity.WeatherRecords);
        _db.Events.Remove(entity);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Event successfully deleted." });
    }

    private static bool IsValidEventType(string value) =>
        new[] { "Run", "Walk", "Cycle" }.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    private static string NormaliseEventType(string value)
    {
        var v = value.Trim().ToLowerInvariant();
        return char.ToUpper(v[0]) + v[1..];
    }
}
