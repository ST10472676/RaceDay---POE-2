using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace RaceDay.Mvc.Models;

public sealed class LoginViewModel { [Required,EmailAddress] public string Email {get;set;}=""; [Required,DataType(DataType.Password)] public string Password {get;set;}=""; }
public sealed class RegisterViewModel { [Required] public string FirstName {get;set;}=""; [Required] public string LastName {get;set;}=""; [Required,EmailAddress] public string Email {get;set;}=""; [Required,MinLength(6),DataType(DataType.Password)] public string Password {get;set;}=""; public string? PhoneNumber {get;set;} [Required] public string Role {get;set;}="Participant"; }
public sealed class EventFormViewModel { public int? EventID {get;set;} [Required] public string EventName {get;set;}=""; public string? Description {get;set;} [Required] public DateTime EventDate {get;set;}=DateTime.Today.AddDays(30); [Required] public string Location {get;set;}=""; [Range(0.01,9999)] public decimal Distance {get;set;}=5; [Required] public string EventType {get;set;}="Run"; [Range(1,int.MaxValue)] public int RouteID {get;set;}=1; public IFormFile? Banner {get;set;} public string? BannerImageUrl {get;set;} }
public sealed class CategoryFormViewModel { public int EventId {get;set;} [Required] public string CategoryName {get;set;}=""; [Range(0,120)] public int MinAge {get;set;} [Range(0,120)] public int MaxAge {get;set;}=120; }
public sealed class EnrolViewModel { public int EventID {get;set;} public int CategoryID {get;set;} }
public sealed class ProfileViewModel { [Required] public string FirstName {get;set;}=""; [Required] public string LastName {get;set;}=""; [Required,EmailAddress] public string Email {get;set;}=""; public string? PhoneNumber {get;set;} public string? ProfilePictureUrl {get;set;} public IFormFile? Picture {get;set;} }
public sealed class ResultFormViewModel { public int EnrolmentID {get;set;} public string FinishTime {get;set;}=""; public int? FinishingPosition {get;set;} public string ResultStatus {get;set;}="Completed"; }
public sealed class ApiPageViewModel { public JsonElement Data {get;set;} public string? Title {get;set;} }

public sealed class EventDashboardItem { public int EventID {get;set;} public string EventName {get;set;}=""; public DateTime EventDate {get;set;} public string Location {get;set;}=""; public decimal Distance {get;set;} public int EnrolmentCount {get;set;} }
public sealed class OrganiserDashboardViewModel { public List<EventDashboardItem> Events {get;set;}=new(); }
