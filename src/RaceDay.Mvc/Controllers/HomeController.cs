using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;
public sealed class HomeController:BaseController
{
 public HomeController(RaceDayApiClient api):base(api){}
 public async Task<IActionResult> Index()
 {
  using var response=await Api.SendAsync(HttpMethod.Get,"api/events");
  if(!response.IsSuccessStatusCode){TempData["Error"]=await RaceDayApiClient.ErrorAsync(response);return View(new ApiPageViewModel{Data=JsonDocument.Parse("[]").RootElement.Clone()});}
  var data=await RaceDayApiClient.ReadAsync<JsonElement>(response);
  return View(new ApiPageViewModel{Data=data});
 }
 public IActionResult Error()=>View();
}
