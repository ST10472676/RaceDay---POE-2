using Microsoft.AspNetCore.Http;

namespace RaceDay.Api.Extensions;

public static class SessionExtensions
{
    public static int? GetCurrentUserId(this HttpContext context) =>
        context.Session.GetInt32("UserId");

    public static string? GetCurrentRole(this HttpContext context) =>
        context.Session.GetString("Role");
}
