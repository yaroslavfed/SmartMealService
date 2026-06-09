using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartMealService.Http.Requests.Menu;
using SmartMealService.Http.Requests.Order;
using SmartMealService.Http.Requests.Order.Parameters;
using SmartMealService.Http.Responses;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Http;

public class SmsHttpClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SmsHttpClient(string baseUrl, string username, string password)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<List<MenuItem>> GetMenuAsync()
    {
        var request = new GetMenuRequest();
        var response = await PostAsync<GetMenuResponse>(request);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return response
               .Data?.MenuItems.Select(dto => new MenuItem
                   {
                       Id = dto.Id,
                       Article = dto.Article,
                       Name = dto.Name,
                       Price = dto.Price,
                       IsWeighted = dto.IsWeighted,
                       FullPath = dto.FullPath,
                       Barcodes = dto.Barcodes
                   }
               )
               .ToList()
               ?? [];
    }

    public async Task<bool> SendOrderAsync(Order order)
    {
        var request = new SendOrderRequest
        {
            CommandParameters = new SendOrderParameters
            {
                OrderId = order.Id,
                MenuItems = order
                            .Items.Select(i => new SendOrderItem { Id = i.Id, Quantity = i.Quantity.ToString("G") })
                            .ToList()
            }
        };

        var response = await PostAsync<SendOrderResponse>(request);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return true;
    }

    private async Task<T> PostAsync<T>(object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync("", content);
        httpResponse.EnsureSuccessStatusCode();

        var responseJson = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, JsonOptions)
               ?? throw new InvalidOperationException("Не удалось десериализовать ответ сервера");
    }
}