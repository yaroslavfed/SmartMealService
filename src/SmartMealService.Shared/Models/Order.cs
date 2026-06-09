namespace SmartMealService.Shared.Models;

public record Order
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public List<OrderItem> Items { get; init; } = [];
}