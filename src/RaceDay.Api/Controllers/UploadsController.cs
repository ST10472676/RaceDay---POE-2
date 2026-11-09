using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IBlobStorageService _blobStorage;
    public UploadsController(RaceDayDbContext db, IBlobStorageService blobStorage)
    { _db = db; _blobStorage = blobStorage; }

    /// <summary>Uploads an event banner to Azure Blob Storage and saves its URL on the event.</summary>
    [HttpPost("events/{eventId:int}/banner")]
    [SessionAuthorize("Organiser")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadEventBanner(int eventId, IFormFile file, CancellationToken cancellationToken)
    {
        var organiserId = HttpContext.GetCurrentUserId()!.Value;
        var eventItem = await _db.Events.SingleOrDefaultAsync(e => e.EventID == eventId, cancellationToken);
        if (eventItem is null) return NotFound(new { message = "Event not found." });
        if (eventItem.OrganiserID != organiserId) return Forbid();
        if (file is null) return BadRequest(new { message = "Select an image file." });
        try
        {
            var uri = await _blobStorage.UploadAsync(file, "event-banners", cancellationToken);
            eventItem.BannerImageUrl = uri.ToString();
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { url = uri.ToString(), eventId });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Uploads the current user's profile picture to Azure Blob Storage.</summary>
    [HttpPost("profile-picture")]
    [SessionAuthorize("Organiser", "Participant")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest(new { message = "Select an image file." });
        var userId = HttpContext.GetCurrentUserId()!.Value;
        var user = await _db.Users.SingleOrDefaultAsync(u => u.UserID == userId, cancellationToken);
        if (user is null) return NotFound();
        try
        {
            var uri = await _blobStorage.UploadAsync(file, "profile-pictures", cancellationToken);
            user.ProfilePictureUrl = uri.ToString();
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { url = uri.ToString() });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
