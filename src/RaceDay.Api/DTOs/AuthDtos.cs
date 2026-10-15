using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record RegisterRequest(
    [property: Required, StringLength(50)] string FirstName,
    [property: Required, StringLength(50)] string LastName,
    [property: Required, EmailAddress, StringLength(100)] string Email,
    [property: Required, MinLength(6), StringLength(100)] string Password,
    [property: StringLength(20)] string? PhoneNumber,
    [property: Required] string Role);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record UserResponse(
    int UserID,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Role,
    DateTime DateCreated);

public record UpdateProfileRequest(
    [property: Required, StringLength(50)] string FirstName,
    [property: Required, StringLength(50)] string LastName,
    [property: Required, EmailAddress, StringLength(100)] string Email,
    [property: StringLength(20)] string? PhoneNumber);
