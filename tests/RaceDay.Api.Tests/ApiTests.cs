using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class ApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RegisterAndLoginParticipant_Succeeds()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "Participant",
            email = $"participant-{Guid.NewGuid():N}@test.co.za",
            password = "Password123!",
            phoneNumber = "0712345678",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        //extract the email from the response rather than relying on a hard-coded account.
        var registered = await register.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(registered);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = registered!.Email,
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("RaceDay.Session", string.Join(";", login.Headers.GetValues("Set-Cookie")));
    }

    [Fact]
    public async Task UnauthenticatedProfile_IsRejected()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrganiserCanCreateEvent()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();

        var organiser = await RegisterAndLogin(client, "Organiser");

        int routeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            var route = new Route
            {
                StartLocation = "Johannesburg",
                EndLocation = "Pretoria",
                RouteDirection = "North",
                Distance = 60m,
                Description = "Test route"
            };
            db.Routes.Add(route);
            await db.SaveChangesAsync();
            routeId = route.RouteID;
        }

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Test Race",
            description = "API test event",
            eventDate = "2027-05-01",
            location = "Johannesburg",
            distance = 60,
            eventType = "Run",
            routeID = routeId,
            bannerImageUrl = (string?)null
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantCannotCreateEvent()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        var participant = await RegisterAndLogin(client, "Participant");

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Blocked Race",
            description = "Should not be created",
            eventDate = "2027-05-01",
            location = "Johannesburg",
            distance = 10,
            eventType = "Run",
            routeID = 1,
            bannerImageUrl = (string?)null
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantCanEnrolAndSeeOwnEnrolment()
    {
        await _factory.ResetDatabaseAsync();
        using var organiserClient = _factory.CreateClient();
        var organiser = await RegisterAndLogin(organiserClient, "Organiser");

        int routeId;
        int eventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            var route = new Route
            {
                StartLocation = "Durban",
                EndLocation = "Pietermaritzburg",
                RouteDirection = "Up Run",
                Distance = 89m,
                Description = "Test ultra route"
            };
            db.Routes.Add(route);
            await db.SaveChangesAsync();
            routeId = route.RouteID;

            var eventEntity = new Event
            {
                OrganiserID = organiser.UserID,
                RouteID = routeId,
                EventName = "Enrolment Test Race",
                Description = "Test event",
                EventDate = new DateTime(2027, 6, 13),
                Location = "KwaZulu-Natal",
                Distance = 89m,
                EventType = "Run"
            };
            db.Events.Add(eventEntity);
            await db.SaveChangesAsync();

            db.Categories.Add(new Category
            {
                EventID = eventEntity.EventID,
                CategoryName = "Senior",
                MinAge = 20,
                MaxAge = 39
            });
            await db.SaveChangesAsync();
            eventId = eventEntity.EventID;
        }

        using var participantClient = _factory.CreateClient();
        var participant = await RegisterAndLogin(participantClient, "Participant");

        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            categoryId = db.Categories.Single(c => c.EventID == eventId).CategoryID;
        }

        var enrol = await participantClient.PostAsJsonAsync("/api/enrolments", new
        {
            eventID = eventId,
            categoryID = categoryId
        });

        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);

        var mine = await participantClient.GetAsync("/api/enrolments/my-enrolments");
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);

        var body = await mine.Content.ReadAsStringAsync();
        Assert.Contains("Enrolment Test Race", body);
    }

    [Fact]
    public async Task WrongPassword_IsRejected()
    {
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateClient();
        var email = $"wrong-password-{Guid.NewGuid():N}@test.co.za";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Wrong",
            lastName = "Password",
            email,
            password = "Password123!",
            phoneNumber = "0712345678",
            role = "Participant"
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    private async Task<UserResponse> RegisterAndLogin(HttpClient client, string role)
    {
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@test.co.za";

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = role,
            email,
            password = "Password123!",
            phoneNumber = "0712345678",
            role
        });

        register.EnsureSuccessStatusCode();
        var user = await register.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);

        await Login(client, user!.Email);
        return user;
    }
    private static async Task Login(HttpClient client, string email)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Password123!"
        });
        login.EnsureSuccessStatusCode();
    }
    private sealed record UserResponse(
        int UserID,
        string FirstName,
        string LastName,
        string Email,
        string? PhoneNumber,
        string Role,
        DateTime DateCreated);
}
