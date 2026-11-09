using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;
public sealed class EventsController:BaseController
{
 public EventsController(RaceDayApiClient api):base(api){}
 public async Task<IActionResult> Details(int id)
 {
  using var response=await Api.SendAsync(HttpMethod.Get,$"api/events/{id}");if(!response.IsSuccessStatusCode){TempData["Error"]=await RaceDayApiClient.ErrorAsync(response);return RedirectToAction("Index","Home");}
  return View(new ApiPageViewModel{Data=(await RaceDayApiClient.ReadAsync<JsonElement>(response))});
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(EnrolViewModel model)
 {
  if(!IsLoggedIn)return RedirectToAction("Login","Account");if(HttpContext.Session.GetString("Role")!="Participant")return RedirectToAction("Index","Home");
  using var response=await Api.SendAsync(HttpMethod.Post,"api/enrolments",new{model.EventID,model.CategoryID});
  if(!response.IsSuccessStatusCode)TempData["Error"]=await RaceDayApiClient.ErrorAsync(response);else TempData["Success"]="Your event enrolment has been submitted.";
  return RedirectToAction(nameof(Details),new{id=model.EventID});
 }
}
