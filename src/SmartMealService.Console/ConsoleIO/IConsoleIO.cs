namespace SmartMealService.Console.ConsoleIO;

public interface IConsoleIO
{
    string? ReadLine();

    void WriteLine(string message);
}
