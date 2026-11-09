using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;
public abstract class BaseController : Controller
{
 protected readonly RaceDayApiClient Api;
 protected BaseController(RaceDayApiClient api) => Api=api;
 protected bool IsLoggedIn => HttpContext.Session.GetInt32("UserId") is not null;
 protected IActionResult? RequireRole(string role) => !IsLoggedIn ? RedirectToAction("Login","Account") : HttpContext.Session.GetString("Role") != role ? RedirectToAction("Index","Home") : null;
}
