using System.Text.Json;
using FluentAssertions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Shared.Tests.Models;

public class MenuItemTests
{
    [Fact]
    public void MenuItem_ShouldHaveCorrectProperties_WhenCreated()
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

        item
            .Should()
            .BeEquivalentTo(
                new MenuItem
                {
                    Id = "5979224",
                    Article = "A1004292",
                    Name = "Каша гречневая",
                    Price = 50,
                    IsWeighted = false,
                    FullPath = @"ПРОИЗВОДСТВО\Гарниры",
                    Barcodes = ["57890975627974236429"]
                }
            );
    }

    [Fact]
    public void MenuItem_Barcodes_ShouldBeEmptyList_ByDefault()
    {
        var item = new MenuItem();

        item
            .Barcodes.Should()
            .NotBeNull();
        item
            .Barcodes.Should()
            .BeEmpty();
    }

    [Fact]
    public void MenuItem_ShouldDeserializeFromJson_Correctly()
    {
        var json = """
                   {
                       "Id": "5979224",
                       "Article": "A1004292",
                       "Name": "Каша гречневая",
                       "Price": 50,
                       "IsWeighted": false,
                       "FullPath": "ПРОИЗВОДСТВО\\Гарниры",
                       "Barcodes": ["57890975627974236429"]
                   }
                   """;

        var item = JsonSerializer.Deserialize<MenuItem>(json);

        item
            .Should()
            .BeEquivalentTo(
                new MenuItem
                {
                    Id = "5979224",
                    Article = "A1004292",
                    Name = "Каша гречневая",
                    Price = 50m,
                    IsWeighted = false,
                    FullPath = @"ПРОИЗВОДСТВО\Гарниры",
                    Barcodes = ["57890975627974236429"]
                }
            );
    }

    [Fact]
    public void MenuItem_ShouldSerializeToJson_WithCorrectPropertyNames()
    {
        var item = new MenuItem { Id = "123", Article = "A001", Name = "Тест", Price = 100 };

        var json = JsonSerializer.Serialize(item);

        json
            .Should()
            .Contain("\"Id\"");
        json
            .Should()
            .Contain("\"Article\"");
        json
            .Should()
            .Contain("\"Name\"");
        json
            .Should()
            .Contain("\"Price\"");
        json
            .Should()
            .Contain("\"IsWeighted\"");
        json
            .Should()
            .Contain("\"Barcodes\"");
    }
}