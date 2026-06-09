using SmartMealService.Shared.Models;
using GrpcOrder = Sms.Test.Order;
using GrpcOrderItem = Sms.Test.OrderItem;

namespace SmartMealService.Grpc.Mapping;

internal static class GrpcOrderMapper
{
    public static GrpcOrder ToGrpcOrder(Order order) =>
        new()
        {
            Id = order.Id,
            OrderItems =
            {
                order.Items.Select(i => new GrpcOrderItem
                {
                    Id = i.Id,
                    Quantity = i.Quantity
                })
            }
        };
}
