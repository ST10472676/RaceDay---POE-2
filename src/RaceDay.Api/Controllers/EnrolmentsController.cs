using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    /// <summary>Creates an enrolment for the authenticated Participant.</summary>
    [HttpPost]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEnrolment(EnrolmentRequest request)
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var eventEntity = await _db.Events.FindAsync(request.EventID);
        if (eventEntity is null) return BadRequest(new { message = "Event not found." });

        var category = await _db.Categories
            .SingleOrDefaultAsync(c => c.CategoryID == request.CategoryID && c.EventID == request.EventID);

        if (category is null)
            return BadRequest(new { message = "The selected category does not belong to the selected event." });

        if (await _db.Enrolments.AnyAsync(e =>
            e.ParticipantID == participantId &&
            e.EventID == request.EventID &&
            e.Status != "Cancelled"))
        {
            return Conflict(new { message = "You are already enrolled for this event." });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            Status = "Pending",
            EnrolmentDate = DateTime.Now
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Created($"/api/enrolments/{enrolment.EnrolmentID}", enrolment);
    }

    /// <summary>Returns all enrolments for the authenticated Participant.</summary>
    [HttpGet("my-enrolments")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var items = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantID == participantId)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .OrderByDescending(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EventID,
                EventName = e.Event.EventName,
                e.CategoryID,
                CategoryName = e.Category.CategoryName,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Returns all enrolments for an event owned by the authenticated Organiser.</summary>
    [HttpGet("~/api/events/{eventId:int}/enrolments")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var eventExists = await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == organiserId);
        if (!eventExists) return NotFound(new { message = "Event not found or not owned by you." });

        var items = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.EventID == eventId)
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Select(e => new
            {
                e.EnrolmentID,
                Participant = new { e.Participant.UserID, e.Participant.FirstName, e.Participant.LastName, e.Participant.Email },
                e.CategoryID,
                CategoryName = e.Category.CategoryName,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Updates an enrolment status for an event owned by the Organiser.</summary>
    [HttpPut("{id:int}/status")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(int id, EnrolmentStatusRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .SingleOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Event.OrganiserID != organiserId) return Forbid();

        var valid = new[] { "Pending", "Confirmed", "Cancelled" };
        if (!valid.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Status must be Pending, Confirmed or Cancelled." });

        enrolment.Status = char.ToUpper(request.Status[0]) + request.Status[1..].ToLowerInvariant();
        await _db.SaveChangesAsync();

        return Ok(enrolment);
    }

    /// <summary>Cancels an enrolment belonging to the authenticated Participant.</summary>
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;
        var enrolment = await _db.Enrolments.SingleOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.ParticipantID != participantId) return Forbid();

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Enrolment successfully cancelled." });
    }
}
