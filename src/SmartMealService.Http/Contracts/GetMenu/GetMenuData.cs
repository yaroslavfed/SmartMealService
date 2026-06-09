namespace SmartMealService.Http.Contracts.GetMenu;

internal record GetMenuData
{
    public List<MenuItemDto> MenuItems { get; init; } = [];
}
