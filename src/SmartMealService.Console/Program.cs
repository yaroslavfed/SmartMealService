using System.Text;
using Serilog;
using SmartMealService.Console.Startup;

System.Console.InputEncoding = Encoding.UTF8;
System.Console.OutputEncoding = Encoding.UTF8;

try
{
    await using var app = ConsoleAppFactory.Create(AppContext.BaseDirectory);
    await app.RunAsync();

    if (!System.Console.IsInputRedirected && !System.Console.IsOutputRedirected)
    {
        const string exitMessage = "Нажмите Enter для выхода...";
        System.Console.WriteLine(exitMessage);
        Log.Information("{Message}", exitMessage);
        System.Console.ReadLine();
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}
