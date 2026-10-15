using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    /// <summary>Returns all categories for a specific event.</summary>
    [HttpGet("api/events/{eventId:int}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.MinAge)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Creates a category for an event owned by the Organiser.</summary>
    [HttpPost("api/events/{eventId:int}/categories")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCategory(int eventId, CategoryRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var eventEntity = await _db.Events.SingleOrDefaultAsync(e => e.EventID == eventId);
        if (eventEntity is null) return NotFound(new { message = "Event not found." });
        if (eventEntity.OrganiserID != organiserId) return Forbid();
        if (request.MaxAge < request.MinAge)
            return BadRequest(new { message = "MaxAge must be greater than or equal to MinAge." });

        var category = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}/categories", category);
    }

    /// <summary>Updates a category belonging to an event owned by the Organiser.</summary>
    [HttpPut("api/categories/{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var category = await _db.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c => c.CategoryID == id);

        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event.OrganiserID != organiserId) return Forbid();
        if (request.MaxAge < request.MinAge)
            return BadRequest(new { message = "MaxAge must be greater than or equal to MinAge." });

        category.CategoryName = request.CategoryName.Trim();
        category.MinAge = request.MinAge;
        category.MaxAge = request.MaxAge;

        await _db.SaveChangesAsync();
        return Ok(category);
    }

    /// <summary>Deletes a category belonging to an event owned by the Organiser.</summary>
    [HttpDelete("api/categories/{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var category = await _db.Categories
            .Include(c => c.Event)
            .Include(c => c.Enrolments)
            .SingleOrDefaultAsync(c => c.CategoryID == id);

        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event.OrganiserID != organiserId) return Forbid();
        if (category.Enrolments.Count > 0)
            return Conflict(new { message = "Cannot delete a category that has enrolments." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Category successfully deleted." });
    }
}
