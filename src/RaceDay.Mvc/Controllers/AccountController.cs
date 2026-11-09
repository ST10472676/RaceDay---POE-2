using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Net;

namespace RaceDay.Mvc.Controllers;
public sealed class AccountController : BaseController
{
 public AccountController(RaceDayApiClient api):base(api){}
 [HttpGet] public IActionResult Login()=>View(new LoginViewModel());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginViewModel model)
 {
  if(!ModelState.IsValid)return View(model);
  using var response=await Api.SendAsync(HttpMethod.Post,"api/auth/login",new{model.Email,model.Password});
  if(!response.IsSuccessStatusCode){ModelState.AddModelError("",await RaceDayApiClient.ErrorAsync(response));return View(model);}
  var user=await RaceDayApiClient.ReadAsync<UserDto>(response);
  if(user is null){ModelState.AddModelError("","The API returned an invalid response.");return View(model);}
  HttpContext.Session.SetInt32("UserId",user.UserID);HttpContext.Session.SetString("Role",user.Role);HttpContext.Session.SetString("FullName",$"{user.FirstName} {user.LastName}");HttpContext.Session.SetString("Email",user.Email);
  return user.Role=="Organiser"?RedirectToAction("Dashboard","Organiser"):RedirectToAction("Index","Home");
 }
 [HttpGet] public IActionResult Register()=>View(new RegisterViewModel());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterViewModel model)
 {
  if(!ModelState.IsValid)return View(model);
  using var response=await Api.SendAsync(HttpMethod.Post,"api/auth/register",new{model.FirstName,model.LastName,model.Email,model.Password,model.PhoneNumber,model.Role});
  if(!response.IsSuccessStatusCode){ModelState.AddModelError("",await RaceDayApiClient.ErrorAsync(response));return View(model);}
  TempData["Success"]="Your account has been created. Please sign in.";return RedirectToAction(nameof(Login));
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout()
 {
  if(HttpContext.Session.GetString("ApiSessionCookie") is not null){using var response=await Api.SendAsync(HttpMethod.Post,"api/auth/logout");}
  HttpContext.Session.Clear();return RedirectToAction("Index","Home");
 }
 public sealed record UserDto(int UserID,string FirstName,string LastName,string Email,string? PhoneNumber,string Role,DateTime DateCreated,string? ProfilePictureUrl);
}
