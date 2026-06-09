using System.Text.Json;
using FluentAssertions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Shared.Tests.Json;

public class JsonContractTests
{
    [Fact]
    public void MenuItem_ShouldSerializeToExpectedJsonShape()
    {
        var item = new MenuItem
        {
            Id = "5979224",
            Article = "A1004292",
            Name = "Каша гречневая",
            Price = 50,
            IsWeighted = false,
            FullPath = @"ПРОИЗВОДСТВО\Гарниры",
            Barcodes = ["57890975627974236429"]
        };

        var json = JsonSerializer.Serialize(item);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.GetProperty("Id").GetString().Should().Be("5979224");
        root.GetProperty("Article").GetString().Should().Be("A1004292");
        root.GetProperty("Name").GetString().Should().Be("Каша гречневая");
        root.GetProperty("Price").GetDecimal().Should().Be(50m);
        root.GetProperty("IsWeighted").GetBoolean().Should().BeFalse();
        root.GetProperty("FullPath").GetString().Should().Be(@"ПРОИЗВОДСТВО\Гарниры");
        root.GetProperty("Barcodes").EnumerateArray().Should().ContainSingle()
            .Which.GetString().Should().Be("57890975627974236429");
    }

    [Fact]
    public void Order_ShouldSerializeToExpectedJsonShape()
    {
        var order = new Order { Id = "62137983-1117-4D10-87C1-EF40A4348250" };
        order.Items.Add(new OrderItem { Id = "5979224", Quantity = 1 });
        order.Items.Add(new OrderItem { Id = "9084246", Quantity = 0.408 });

        var json = JsonSerializer.Serialize(order);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var items = root.GetProperty("Items").EnumerateArray().ToList();

        root.GetProperty("Id").GetString().Should().Be("62137983-1117-4D10-87C1-EF40A4348250");
        items.Should().HaveCount(2);
        items[0].GetProperty("Id").GetString().Should().Be("5979224");
        items[0].GetProperty("Quantity").GetDouble().Should().Be(1);
        items[1].GetProperty("Id").GetString().Should().Be("9084246");
        items[1].GetProperty("Quantity").GetDouble().Should().BeApproximately(0.408, 0.0001);
    }

    [Fact]
    public void Order_ShouldDeserializeFromJsonShape()
    {
        const string json = """
                            {
                                "Id": "62137983-1117-4D10-87C1-EF40A4348250",
                                "Items": [
                                    { "Id": "5979224", "Quantity": 1 },
                                    { "Id": "9084246", "Quantity": 0.408 }
                                ]
                            }
                            """;

        var order = JsonSerializer.Deserialize<Order>(json);

        order.Should().NotBeNull();
        order!.Id.Should().Be("62137983-1117-4D10-87C1-EF40A4348250");
        order.Items.Should().HaveCount(2);
        order.Items[0].Should().BeEquivalentTo(new OrderItem { Id = "5979224", Quantity = 1 });
        order.Items[1].Id.Should().Be("9084246");
        order.Items[1].Quantity.Should().BeApproximately(0.408, 0.0001);
    }
}
