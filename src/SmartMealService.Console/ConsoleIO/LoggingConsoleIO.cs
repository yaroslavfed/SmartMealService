using Serilog;

namespace SmartMealService.Console.ConsoleIO;

public class LoggingConsoleIO : IConsoleIO
{
    public string? ReadLine()
    {
        var input = global::System.Console.ReadLine();
        Log.Information("Ввод пользователя: {Input}", input);
        return input;
    }

    public void WriteLine(string message)
    {
        global::System.Console.WriteLine(message);
        Log.Information("{Message}", message);
    }
}
