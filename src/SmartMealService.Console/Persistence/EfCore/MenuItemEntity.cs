using System.Text.Json;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Persistence.EfCore;

public class MenuItemEntity
{
    public string Id { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsWeighted { get; set; }
    public string FullPath { get; set; } = string.Empty;
    public string BarcodesJson { get; set; } = "[]";

    public List<string> Barcodes
    {
        get => JsonSerializer.Deserialize<List<string>>(BarcodesJson) ?? [];
        set => BarcodesJson = JsonSerializer.Serialize(value);
    }

    public static MenuItemEntity FromModel(MenuItem item) =>
        new()
        {
            Id = item.Id,
            Article = item.Article,
            Name = item.Name,
            Price = item.Price,
            IsWeighted = item.IsWeighted,
            FullPath = item.FullPath,
            Barcodes = item.Barcodes
        };
}
