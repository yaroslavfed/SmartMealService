using Serilog;

namespace SmartMealService.Console.ConsoleIO;

public class LoggingConsoleIO : IConsoleIO
{
    public string? ReadLine()
    {
        return global::System.Console.ReadLine();
    }

    public void WriteLine(string message)
    {
        global::System.Console.WriteLine(message);
        Log.Information("{Message}", message);
    }
}
