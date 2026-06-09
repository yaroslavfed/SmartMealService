using System.Globalization;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Services;

public static class OrderInputParser
{
    public static List<OrderItem> Parse(string? input, IReadOnlyCollection<MenuItem> availableItems)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new OrderInputException("Order input cannot be empty.");

        if (availableItems.Count == 0)
            throw new OrderInputException("Menu is empty.");

        var menuIds = availableItems.Select(i => i.Id).ToHashSet(StringComparer.Ordinal);
        var result = new List<OrderItem>();

        foreach (var rawPart in input.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = rawPart.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                throw new OrderInputException($"Invalid order item format: {rawPart}");

            var id = parts[0];
            if (!menuIds.Contains(id))
                throw new OrderInputException($"Menu item with id {id} does not exist.");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var quantity) ||
                quantity <= 0)
            {
                throw new OrderInputException($"Invalid quantity for menu item {id}.");
            }

            result.Add(new OrderItem { Id = id, Quantity = quantity });
        }

        if (result.Count == 0)
            throw new OrderInputException("Order input does not contain items.");

        return result;
    }
}
