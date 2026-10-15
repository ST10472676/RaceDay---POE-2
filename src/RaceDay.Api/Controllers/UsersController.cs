using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[SessionAuthorize("Organiser", "Participant")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db) => _db = db;

    /// <summary>Returns the authenticated user's own profile.</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound(new { message = "User profile not found." });

        return Ok(ToResponse(user));
    }

    /// <summary>Updates the authenticated user's profile without allowing a role change.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound(new { message = "User profile not found." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != userId))
            return Conflict(new { message = "That email address is already in use." });

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.PhoneNumber = request.PhoneNumber?.Trim();

        await _db.SaveChangesAsync();
        HttpContext.Session.SetString("Email", user.Email);

        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(User user) =>
        new(user.UserID, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Role, user.DateCreated);
}
