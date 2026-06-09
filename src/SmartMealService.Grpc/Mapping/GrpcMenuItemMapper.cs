using SmartMealService.Shared.Models;
using GrpcMenuItem = Sms.Test.MenuItem;

namespace SmartMealService.Grpc.Mapping;

internal static class GrpcMenuItemMapper
{
    public static MenuItem ToModel(GrpcMenuItem item) =>
        new()
        {
            Id = item.Id,
            Article = item.Article,
            Name = item.Name,
            Price = (decimal)item.Price,
            IsWeighted = item.IsWeighted,
            FullPath = item.FullPath,
            Barcodes = [..item.Barcodes]
        };
}
