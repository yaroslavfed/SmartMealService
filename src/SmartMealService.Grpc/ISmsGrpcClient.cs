using SmartMealService.Shared.Models;

namespace SmartMealService.Grpc;

public interface ISmsGrpcClient
{
    Task<List<MenuItem>> GetMenuAsync();
    Task<bool> SendOrderAsync(Order order);
}