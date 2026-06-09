namespace SmartMealService.Shared.Models;

/// <summary>
/// Заказ, который приложение формирует из выбранных пользователем блюд и отправляет в SMS.
/// </summary>
public record Order
{
    /// <summary>Уникальный идентификатор заказа, передаваемый серверу SMS.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Позиции заказа: код блюда и количество.</summary>
    public List<OrderItem> Items { get; init; } = [];
}
