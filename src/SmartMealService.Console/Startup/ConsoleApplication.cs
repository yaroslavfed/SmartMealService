using SmartMealService.Console.Ordering;
using SmartMealService.Console.Persistence.EfCore;

namespace SmartMealService.Console.Startup;

public sealed class ConsoleApplication(
    OrderConsoleRunner runner,
    MenuDbContext dbContext) : IAsyncDisposable
{
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }
}
