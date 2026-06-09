using System.Globalization;
using SmartMealService.Console.Exceptions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Input;

public static class OrderInputParser
{
    public static List<OrderItem> Parse(string? input, IReadOnlyCollection<MenuItem> availableItems)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new OrderInputException("Строка заказа не может быть пустой.");

        if (availableItems.Count == 0)
            throw new OrderInputException("Меню пустое.");

        var menuIds = availableItems.Select(i => i.Id).ToHashSet(StringComparer.Ordinal);
        var result = new List<OrderItem>();

        foreach (var rawPart in input.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = rawPart.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                throw new OrderInputException($"Некорректный формат позиции заказа: {rawPart}");

            var id = parts[0];
            if (!menuIds.Contains(id))
                throw new OrderInputException($"Блюдо с кодом {id} не найдено в меню.");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var quantity))
                throw new OrderInputException($"Количество для блюда {id} должно быть числом.");

            if (quantity <= 0)
                throw new OrderInputException($"Количество для блюда {id} должно быть больше нуля.");

            result.Add(new OrderItem { Id = id, Quantity = quantity });
        }

        if (result.Count == 0)
            throw new OrderInputException("Строка заказа не содержит позиций.");

        return result;
    }
}
