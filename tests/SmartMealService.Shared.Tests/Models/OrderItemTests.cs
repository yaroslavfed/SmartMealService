using FluentAssertions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Shared.Tests.Models;

public class OrderItemTests
{
    [Fact]
    public void OrderItem_ShouldHaveCorrectProperties_WhenCreated()
    {
        var item = new OrderItem { Id = "5979224", Quantity = 1.0 };

        item.Id.Should().Be("5979224");
        item.Quantity.Should().Be(1.0);
    }

    [Fact]
    public void OrderItem_ShouldSupportFractionalQuantity()
    {
        var item = new OrderItem { Id = "9084246", Quantity = 0.408 };

        item.Quantity.Should().BeApproximately(0.408, precision: 0.0001);
    }
}