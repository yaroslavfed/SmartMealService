using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Persistence;

public interface IMenuRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task SaveMenuAsync(IEnumerable<MenuItem> menuItems, CancellationToken cancellationToken = default);
}
