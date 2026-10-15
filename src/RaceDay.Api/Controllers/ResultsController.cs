using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    /// <summary>Records a result for an enrolment belonging to the Organiser's event.</summary>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateResult(ResultRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .SingleOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentID);

        if (enrolment is null)
            return BadRequest(new { message = "Enrolment not found." });
        if (enrolment.Event.OrganiserID != organiserId)
            return Forbid();
        if (await _db.Results.AnyAsync(r => r.EnrolmentID == request.EnrolmentID))
            return Conflict(new { message = "A result already exists for this enrolment." });
        if (!IsValidStatus(request.ResultStatus))
            return BadRequest(new { message = "ResultStatus must be Completed, DNF or DNS." });

        var result = new Result
        {
            EnrolmentID = request.EnrolmentID,
            FinishTime = request.FinishTime,
            FinishingPosition = request.FinishingPosition,
            ResultStatus = NormaliseStatus(request.ResultStatus)
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return Created($"/api/results/{result.ResultID}", result);
    }

    /// <summary>Returns the authenticated Participant's race history.</summary>
    [HttpGet("my-results")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyResults()
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var results = await _db.Results
            .AsNoTracking()
            .Where(r => r.Enrolment.ParticipantID == participantId)
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .Select(r => new
            {
                r.ResultID,
                EventName = r.Enrolment.Event.EventName,
                EventDate = r.Enrolment.Event.EventDate,
                Category = r.Enrolment.Category.CategoryName,
                r.FinishTime,
                r.FinishingPosition,
                r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Returns all results for an event owned by the authenticated Organiser.</summary>
    [HttpGet("~/api/events/{eventId:int}/results")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var eventExists = await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == organiserId);
        if (!eventExists) return NotFound(new { message = "Event not found or not owned by you." });

        var results = await _db.Results
            .AsNoTracking()
            .Where(r => r.Enrolment.EventID == eventId)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .Select(r => new
            {
                r.ResultID,
                r.EnrolmentID,
                Participant = new
                {
                    r.Enrolment.Participant.UserID,
                    r.Enrolment.Participant.FirstName,
                    r.Enrolment.Participant.LastName
                },
                Category = r.Enrolment.Category.CategoryName,
                r.FinishTime,
                r.FinishingPosition,
                r.ResultStatus
            })
            .OrderBy(r => r.FinishingPosition)
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Updates a result for an enrolment belonging to the Organiser's event.</summary>
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateResult(int id, ResultUpdateRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .SingleOrDefaultAsync(r => r.ResultID == id);

        if (result is null) return NotFound(new { message = "Result not found." });
        if (result.Enrolment.Event.OrganiserID != organiserId) return Forbid();
        if (!IsValidStatus(request.ResultStatus))
            return BadRequest(new { message = "ResultStatus must be Completed, DNF or DNS." });

        result.FinishTime = request.FinishTime;
        result.FinishingPosition = request.FinishingPosition;
        result.ResultStatus = NormaliseStatus(request.ResultStatus);

        await _db.SaveChangesAsync();
        return Ok(result);
    }

    private static bool IsValidStatus(string value) =>
        new[] { "Completed", "DNF", "DNS" }.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    private static string NormaliseStatus(string value)
    {
        var v = value.Trim().ToUpperInvariant();
        return v switch
        {
            "COMPLETED" => "Completed",
            "DNF" => "DNF",
            "DNS" => "DNS",
            _ => value
        };
    }
}
