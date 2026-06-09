using FluentAssertions;
using SmartMealService.Console.Exceptions;
using SmartMealService.Console.Input;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Tests.Input;

public class OrderInputParserTests
{
    private readonly List<MenuItem> _availableItems =
    [
        new MenuItem { Id = "5979224", Article = "A1004292", Name = "Каша гречневая", Price = 50 },
        new MenuItem { Id = "9084246", Article = "A1004293", Name = "Конфеты Коровка", Price = 300 },
        new MenuItem { Id = "1111111", Article = "A1004294", Name = "Суп куриный", Price = 120 }
    ];

    [Fact]
    public void Parse_ShouldReturnOrderItems_WhenInputIsValid()
    {
        var result = OrderInputParser.Parse("5979224:2;9084246:1", _availableItems);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("5979224");
        result[0].Quantity.Should().Be(2);
        result[1].Id.Should().Be("9084246");
        result[1].Quantity.Should().Be(1);
    }

    [Fact]
    public void Parse_ShouldSupportFractionalQuantity()
    {
        var result = OrderInputParser.Parse("9084246:0.408", _availableItems);

        result.Should().ContainSingle();
        result[0].Quantity.Should().BeApproximately(0.408, 0.0001);
    }

    [Fact]
    public void Parse_ShouldSupportSingleItem()
    {
        var result = OrderInputParser.Parse("1111111:3", _availableItems);

        result.Should().ContainSingle()
            .Which.Id.Should().Be("1111111");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenCodeDoesNotExist()
    {
        var act = () => OrderInputParser.Parse("9999999:1", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Блюдо с кодом 9999999 не найдено в меню.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenOneOfCodesDoesNotExist()
    {
        var act = () => OrderInputParser.Parse("5979224:1;9999999:2", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Блюдо с кодом 9999999 не найдено в меню.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenQuantityIsZero()
    {
        var act = () => OrderInputParser.Parse("5979224:0", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Количество для блюда 5979224 должно быть больше нуля.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenQuantityIsNegative()
    {
        var act = () => OrderInputParser.Parse("5979224:-1", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Количество для блюда 5979224 должно быть больше нуля.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenQuantityIsNotANumber()
    {
        var act = () => OrderInputParser.Parse("5979224:abc", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Количество для блюда 5979224 должно быть числом.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenFormatIsInvalid()
    {
        var act = () => OrderInputParser.Parse("невалидный ввод", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Некорректный формат позиции заказа: невалидный ввод");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenInputIsEmpty()
    {
        var act = () => OrderInputParser.Parse("", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Строка заказа не может быть пустой.");
    }

    [Fact]
    public void Parse_ShouldThrowException_WhenInputIsWhitespace()
    {
        var act = () => OrderInputParser.Parse("   ", _availableItems);

        act.Should().Throw<OrderInputException>()
            .WithMessage("Строка заказа не может быть пустой.");
    }
}
