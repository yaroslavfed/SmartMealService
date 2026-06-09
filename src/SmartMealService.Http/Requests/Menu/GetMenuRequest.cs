using SmartMealService.Http.Requests.Menu.Parameters;

namespace SmartMealService.Http.Requests.Menu;

internal record GetMenuRequest
{
    public string Command { get; init; } = "GetMenu";
    public GetMenuParameters CommandParameters { get; init; } = new();
}