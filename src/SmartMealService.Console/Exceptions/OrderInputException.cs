namespace SmartMealService.Console.Exceptions;

public sealed class OrderInputException : Exception
{
    public OrderInputException(string message) : base(message) { }
}