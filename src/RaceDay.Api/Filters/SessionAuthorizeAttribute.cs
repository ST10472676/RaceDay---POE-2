using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles ?? Array.Empty<string>();
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var role = session.GetString("Role");

        if (userId is null || string.IsNullOrWhiteSpace(role))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Authentication required. Please log in."
            });
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                message = "You do not have permission to access this resource."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
