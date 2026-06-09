using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartMealService.Http.Transport;

internal sealed class JsonPostEndpointClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public JsonPostEndpointClient(string baseUrl, string username, string password)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<T> PostAsync<T>(object body, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync("", content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(responseJson, JsonOptions)
               ?? throw new InvalidOperationException("Не удалось десериализовать ответ сервера");
    }
}
