using Serilog;
using SmartMealService.Console.Startup;

try
{
    await using var app = ConsoleAppFactory.Create(AppContext.BaseDirectory);
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}
