namespace SmartMealService.Http.Contracts.GetMenu;

internal record GetMenuRequest
{
    public string Command { get; init; } = "GetMenu";
    public GetMenuParameters CommandParameters { get; init; } = new();
}
