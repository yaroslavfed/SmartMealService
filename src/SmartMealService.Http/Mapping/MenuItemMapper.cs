using SmartMealService.Http.Contracts.GetMenu;
using SmartMealService.Shared.Models;

namespace SmartMealService.Http.Mapping;

internal static class MenuItemMapper
{
    public static MenuItem ToModel(MenuItemDto dto) =>
        new()
        {
            Id = dto.Id,
            Article = dto.Article,
            Name = dto.Name,
            Price = dto.Price,
            IsWeighted = dto.IsWeighted,
            FullPath = dto.FullPath,
            Barcodes = dto.Barcodes
        };
}
