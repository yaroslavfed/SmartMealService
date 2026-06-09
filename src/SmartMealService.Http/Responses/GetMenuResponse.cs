namespace SmartMealService.Http.Responses;

internal record GetMenuResponse
{
    public string Command { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public GetMenuData? Data { get; init; }
}