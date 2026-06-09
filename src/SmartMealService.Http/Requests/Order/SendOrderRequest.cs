using SmartMealService.Http.Requests.Order.Parameters;

namespace SmartMealService.Http.Requests.Order;

internal record SendOrderRequest
{
    public string Command { get; init; } = "SendOrder";
    public SendOrderParameters CommandParameters { get; init; } = new();
}