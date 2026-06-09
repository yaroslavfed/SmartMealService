using Autofac;
using SmartMealService.Console.Ordering;

namespace SmartMealService.Console.Startup;

public sealed class ConsoleApplication(IContainer container) : IAsyncDisposable
{
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return container.Resolve<OrderConsoleRunner>().RunAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        container.Dispose();
        return ValueTask.CompletedTask;
    }
}
