using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;
public sealed class ParticipantController:BaseController
{
 public ParticipantController(RaceDayApiClient api):base(api){}
 public async Task<IActionResult> MyEvents(){if(RequireRole("Participant") is IActionResult r)return r;using var res=await Api.SendAsync(HttpMethod.Get,"api/enrolments/my-enrolments");return View(new ApiPageViewModel{Data=await RaceDayApiClient.ReadAsync<JsonElement>(res)});}
 public async Task<IActionResult> MyResults(){if(RequireRole("Participant") is IActionResult r)return r;using var res=await Api.SendAsync(HttpMethod.Get,"api/results/my-results");return View(new ApiPageViewModel{Data=await RaceDayApiClient.ReadAsync<JsonElement>(res)});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> CancelEnrolment(int id){if(RequireRole("Participant") is IActionResult r)return r;using var res=await Api.SendAsync(HttpMethod.Delete,$"api/enrolments/{id}");TempData[res.IsSuccessStatusCode?"Success":"Error"]=res.IsSuccessStatusCode?"Enrolment cancelled.":await RaceDayApiClient.ErrorAsync(res);return RedirectToAction(nameof(MyEvents));}
}
