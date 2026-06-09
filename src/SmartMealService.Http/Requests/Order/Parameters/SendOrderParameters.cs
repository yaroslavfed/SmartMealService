namespace SmartMealService.Http.Requests.Order.Parameters;

internal record SendOrderParameters
{
    public string OrderId { get; init; } = string.Empty;
    public List<SendOrderItem> MenuItems { get; init; } = [];
}