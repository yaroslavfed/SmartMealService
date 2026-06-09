using FluentAssertions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Shared.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Order_ShouldHaveNonEmptyId_WhenCreated()
    {
        var order = new Order();

        order
            .Id.Should()
            .NotBeNullOrEmpty();
    }

    [Fact]
    public void Order_Id_ShouldBeValidGuid()
    {
        var order = new Order();

        Guid
            .TryParse(order.Id, out _)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Order_Items_ShouldBeEmptyList_ByDefault()
    {
        var order = new Order();

        order
            .Items.Should()
            .NotBeNull();
        order
            .Items.Should()
            .BeEmpty();
    }

    [Fact]
    public void Order_ShouldAllowAddingItems()
    {
        var order = new Order();
        var item = new OrderItem { Id = "5979224", Quantity = 2 };

        order.Items.Add(item);

        order
            .Items.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new OrderItem { Id = "5979224", Quantity = 2 });
    }

    [Fact]
    public void Order_EachInstance_ShouldHaveUniqueId()
    {
        var order1 = new Order();
        var order2 = new Order();

        order1
            .Id.Should()
            .NotBe(order2.Id);
    }
}