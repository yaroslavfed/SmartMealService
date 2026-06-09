namespace SmartMealService.Http.Requests.Menu.Parameters;

internal record GetMenuParameters
{
    public bool WithPrice { get; init; } = true;
}