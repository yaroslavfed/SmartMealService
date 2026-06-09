using SmartMealService.Http.Contracts.GetMenu;
using SmartMealService.Http.Contracts.SendOrder;
using SmartMealService.Http.Mapping;
using SmartMealService.Http.Transport;
using SmartMealService.Shared.Abstractions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Http;

public class SmsHttpClient : ISmsClient
{
    private readonly JsonPostEndpointClient _endpointClient;

    public SmsHttpClient(string baseUrl, string username, string password)
    {
        _endpointClient = new JsonPostEndpointClient(baseUrl, username, password);
    }

    public async Task<List<MenuItem>> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        var response = await _endpointClient.PostAsync<GetMenuResponse>(
            new GetMenuRequest(),
            cancellationToken);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return response.Data?.MenuItems.Select(MenuItemMapper.ToModel).ToList() ?? [];
    }

    public async Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var response = await _endpointClient.PostAsync<SendOrderResponse>(
            SendOrderRequestFactory.Create(order),
            cancellationToken);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return true;
    }
}
