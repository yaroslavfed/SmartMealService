namespace SmartMealService.Shared.Models;

/// <summary>
/// Одна позиция заказа: выбранное блюдо и заказанное количество.
/// </summary>
public record OrderItem
{
    /// <summary>Код блюда из меню SMS.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Количество блюда; для весовых блюд допускается дробное значение.</summary>
    public double Quantity { get; init; }
}
