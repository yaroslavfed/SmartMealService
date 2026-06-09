namespace SmartMealService.Console.Services;

public sealed class OrderInputException : Exception
{
    public OrderInputException(string message) : base(message)
    {
    }
}
