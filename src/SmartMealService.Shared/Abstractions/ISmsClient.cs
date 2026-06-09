using SmartMealService.Shared.Models;

namespace SmartMealService.Shared.Abstractions;

/// <summary>
/// Общий контракт клиента SMS API независимо от транспорта: HTTP или gRPC.
/// </summary>
public interface ISmsClient
{
    /// <summary>
    /// Получает актуальное меню блюд из SMS.
    /// </summary>
    Task<List<MenuItem>> GetMenuAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправляет сформированный заказ в SMS.
    /// </summary>
    Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken = default);
}
