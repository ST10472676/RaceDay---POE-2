using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

/// <summary>HTTP client facade. API session cookies are stored in the MVC session and replayed to the API.</summary>
public sealed class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    { _factory = factory; _accessor = accessor; }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = new HttpRequestMessage(method, path);
        var apiCookie = _accessor.HttpContext?.Session.GetString("ApiSessionCookie");
        if (!string.IsNullOrWhiteSpace(apiCookie)) request.Headers.Add("Cookie", apiCookie);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        var response = await client.SendAsync(request, ct);
        SaveApiCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> SendFileAsync(string path, IFormFile file, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        var apiCookie = _accessor.HttpContext?.Session.GetString("ApiSessionCookie");
        if (!string.IsNullOrWhiteSpace(apiCookie)) request.Headers.Add("Cookie", apiCookie);
        var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", Path.GetFileName(file.FileName));
        request.Content = content;
        var response = await client.SendAsync(request, ct);
        SaveApiCookie(response);
        return response;
    }

    private void SaveApiCookie(HttpResponseMessage response)
    {
        if (_accessor.HttpContext is null || !response.Headers.TryGetValues("Set-Cookie", out var cookies)) return;
        var cookie = cookies.FirstOrDefault(x => x.StartsWith(".RaceDay.Session=", StringComparison.OrdinalIgnoreCase));
        if (cookie is not null) _accessor.HttpContext.Session.SetString("ApiSessionCookie", cookie.Split(';', 2)[0]);
    }

    public static async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response.StatusCode == HttpStatusCode.NoContent) return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }
    public static async Task<string> ErrorAsync(HttpResponseMessage response)
    {
        try { using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? "The API request failed.";
            if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString() ?? "Please check the information supplied.";
        } catch { }
        return $"The request could not be completed (HTTP {(int)response.StatusCode}).";
    }
}
