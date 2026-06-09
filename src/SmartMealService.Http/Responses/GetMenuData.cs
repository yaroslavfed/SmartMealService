namespace SmartMealService.Http.Responses;

internal record GetMenuData
{
    public List<MenuItemDto> MenuItems { get; init; } = [];
}