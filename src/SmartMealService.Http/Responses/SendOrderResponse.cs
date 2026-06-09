namespace SmartMealService.Http.Responses;

internal record SendOrderResponse
{
    public string Command { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
}