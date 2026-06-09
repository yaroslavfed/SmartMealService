using Microsoft.EntityFrameworkCore;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Persistence.EfCore;

public class EfMenuRepository(MenuDbContext dbContext) : IMenuRepository
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task SaveMenuAsync(IEnumerable<MenuItem> menuItems, CancellationToken cancellationToken = default)
    {
        dbContext.MenuItems.RemoveRange(dbContext.MenuItems);
        await dbContext.MenuItems.AddRangeAsync(menuItems.Select(MenuItemEntity.FromModel), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
