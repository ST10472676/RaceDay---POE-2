using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;
public sealed class ProfileController:BaseController
{
 public ProfileController(RaceDayApiClient api):base(api){}
 [HttpGet] public async Task<IActionResult> Index(){if(!IsLoggedIn)return RedirectToAction("Login","Account");using var res=await Api.SendAsync(HttpMethod.Get,"api/users/profile");if(!res.IsSuccessStatusCode){TempData["Error"]=await RaceDayApiClient.ErrorAsync(res);return RedirectToAction("Index","Home");}var u=await RaceDayApiClient.ReadAsync<JsonElement>(res);return View(new ProfileViewModel{FirstName=u.GetProperty("firstName").GetString()??"",LastName=u.GetProperty("lastName").GetString()??"",Email=u.GetProperty("email").GetString()??"",PhoneNumber=u.TryGetProperty("phoneNumber",out var p)?p.GetString():null,ProfilePictureUrl=u.TryGetProperty("profilePictureUrl",out var pic)?pic.GetString():null});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(ProfileViewModel m){if(!IsLoggedIn)return RedirectToAction("Login","Account");if(!ModelState.IsValid)return View(m);using var res=await Api.SendAsync(HttpMethod.Put,"api/users/profile",new{m.FirstName,m.LastName,m.Email,m.PhoneNumber});if(!res.IsSuccessStatusCode){ModelState.AddModelError("",await RaceDayApiClient.ErrorAsync(res));return View(m);}if(m.Picture is not null){using var upload=await Api.SendFileAsync("api/uploads/profile-picture",m.Picture);if(!upload.IsSuccessStatusCode)TempData["Error"]=await RaceDayApiClient.ErrorAsync(upload);else{var data=await RaceDayApiClient.ReadAsync<JsonElement>(upload);m.ProfilePictureUrl=data.GetProperty("url").GetString();}}HttpContext.Session.SetString("FullName",$"{m.FirstName} {m.LastName}");HttpContext.Session.SetString("Email",m.Email);TempData["Success"]="Profile updated.";return View(m);}
}
