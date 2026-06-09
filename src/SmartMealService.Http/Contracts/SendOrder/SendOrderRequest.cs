namespace SmartMealService.Http.Contracts.SendOrder;

internal record SendOrderRequest
{
    public string Command { get; init; } = "SendOrder";
    public SendOrderParameters CommandParameters { get; init; } = new();
}
