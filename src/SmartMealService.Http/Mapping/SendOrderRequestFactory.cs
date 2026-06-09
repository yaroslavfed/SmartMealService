using SmartMealService.Http.Contracts.SendOrder;
using SmartMealService.Shared.Models;

namespace SmartMealService.Http.Mapping;

internal static class SendOrderRequestFactory
{
    public static SendOrderRequest Create(Order order) =>
        new()
        {
            CommandParameters = new SendOrderParameters
            {
                OrderId = order.Id,
                MenuItems = order.Items
                    .Select(i => new SendOrderItem { Id = i.Id, Quantity = i.Quantity.ToString("G") })
                    .ToList()
            }
        };
}
