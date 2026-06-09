namespace SmartMealService.Shared.Models;

public record OrderItem
{
    public string Id { get; init; } = string.Empty;

    public double Quantity { get; init; }
}