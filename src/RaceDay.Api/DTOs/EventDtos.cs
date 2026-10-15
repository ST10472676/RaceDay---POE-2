using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record EventRequest(
    [property: Required, StringLength(150)] string EventName,
    [property: StringLength(1000)] string? Description,
    [property: Required] DateTime EventDate,
    [property: Required, StringLength(150)] string Location,
    [property: Range(0.01, 9999)] decimal Distance,
    [property: Required] string EventType,
    [property: Range(1, int.MaxValue)] int RouteID,
    [property: StringLength(500)] string? BannerImageUrl);

public record CategoryRequest(
    [property: Required, StringLength(100)] string CategoryName,
    [property: Range(0, 120)] int MinAge,
    [property: Range(0, 120)] int MaxAge);

public record EnrolmentRequest(
    [property: Range(1, int.MaxValue)] int EventID,
    [property: Range(1, int.MaxValue)] int CategoryID);

public record EnrolmentStatusRequest(
    [property: Required] string Status);

public record ResultRequest(
    [property: Range(1, int.MaxValue)] int EnrolmentID,
    TimeSpan? FinishTime,
    [property: Range(1, int.MaxValue)] int? FinishingPosition,
    [property: Required] string ResultStatus);

public record ResultUpdateRequest(
    TimeSpan? FinishTime,
    [property: Range(1, int.MaxValue)] int? FinishingPosition,
    [property: Required] string ResultStatus);

public record WeatherRequest(
    [property: Required] DateTime WeatherDate,
    decimal? Temperature,
    [property: StringLength(100)] string? WeatherCondition,
    [property: Range(0, 100)] int? Humidity);
