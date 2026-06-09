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
        const string exitMessage = "\u041d\u0430\u0436\u043c\u0438\u0442\u0435 Enter \u0434\u043b\u044f \u0432\u044b\u0445\u043e\u0434\u0430...";
        System.Console.WriteLine(exitMessage);
        Log.Information("{Message}", exitMessage);
        System.Console.ReadLine();
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}
