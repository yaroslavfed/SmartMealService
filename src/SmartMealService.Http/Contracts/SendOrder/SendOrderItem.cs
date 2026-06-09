namespace SmartMealService.Http.Contracts.SendOrder;

internal record SendOrderItem
{
    public string Id { get; init; } = string.Empty;
    public string Quantity { get; init; } = string.Empty;
}
