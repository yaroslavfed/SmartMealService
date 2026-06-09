namespace SmartMealService.Http.Contracts.GetMenu;

internal record GetMenuParameters
{
    public bool WithPrice { get; init; } = true;
}
