namespace SmartMealService.Shared.Models;

/// <summary>
/// Блюдо из меню SMS: общая доменная модель, которую используют HTTP, gRPC, консольное и WPF-приложения.
/// </summary>
public record MenuItem
{
    /// <summary>Код блюда на стороне SMS.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Артикул блюда.</summary>
    public string Article { get; init; } = string.Empty;

    /// <summary>Название блюда, выводимое пользователю.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Цена за единицу блюда.</summary>
    public decimal Price { get; init; }

    /// <summary>Признак весового блюда, для которого количество может быть дробным.</summary>
    public bool IsWeighted { get; init; }

    /// <summary>Полный путь блюда в иерархии меню SMS.</summary>
    public string FullPath { get; init; } = string.Empty;

    /// <summary>Список штрихкодов, связанных с блюдом.</summary>
    public List<string> Barcodes { get; init; } = [];
}
