using FluentAssertions;
using Moq;
using SmartMealService.Console.ConsoleIO;
using SmartMealService.Console.Ordering;
using SmartMealService.Console.Persistence;
using SmartMealService.Shared.Abstractions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Tests.Ordering;

public class OrderConsoleRunnerTests
{
    [Fact]
    public async Task RunAsync_ShouldStop_WhenGetMenuReturnsBusinessError()
    {
        var smsClient = new Mock<ISmsClient>();
        var menuRepository = new Mock<IMenuRepository>();
        var console = new TestConsoleIO();
        var runner = new OrderConsoleRunner(smsClient.Object, menuRepository.Object, console);

        menuRepository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        smsClient.Setup(c => c.GetMenuAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmsApiException("Menu unavailable"));

        await runner.RunAsync();

        console.Output.Should().Contain("Menu unavailable");
        menuRepository.Verify(r => r.SaveMenuAsync(It.IsAny<IEnumerable<MenuItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        smsClient.Verify(c => c.SendOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ShouldAskAgain_WhenInputIsInvalid()
    {
        var smsClient = new Mock<ISmsClient>();
        var menuRepository = new Mock<IMenuRepository>();
        var console = new TestConsoleIO("bad input", "5979224:1;9084246:0.408");
        var runner = new OrderConsoleRunner(smsClient.Object, menuRepository.Object, console);
        Order? sentOrder = null;

        menuRepository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        menuRepository.Setup(r => r.SaveMenuAsync(It.IsAny<IEnumerable<MenuItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        smsClient.Setup(c => c.GetMenuAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Menu());
        smsClient.Setup(c => c.SendOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => sentOrder = order)
            .ReturnsAsync(true);

        await runner.RunAsync();

        console.Output.Should().Contain(message => message.Contains("Invalid order item format"));
        console.Output.Should().Contain("\u0423\u0421\u041f\u0415\u0425");
        sentOrder.Should().NotBeNull();
        sentOrder!.Items.Should().HaveCount(2);
        sentOrder.Items[0].Should().BeEquivalentTo(new OrderItem { Id = "5979224", Quantity = 1 });
        sentOrder.Items[1].Id.Should().Be("9084246");
        sentOrder.Items[1].Quantity.Should().BeApproximately(0.408, 0.0001);
    }

    [Fact]
    public async Task RunAsync_ShouldWriteSuccess_WhenOrderIsSent()
    {
        var smsClient = new Mock<ISmsClient>();
        var menuRepository = new Mock<IMenuRepository>();
        var console = new TestConsoleIO("5979224:1");
        var runner = new OrderConsoleRunner(smsClient.Object, menuRepository.Object, console);

        menuRepository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        menuRepository.Setup(r => r.SaveMenuAsync(It.IsAny<IEnumerable<MenuItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        smsClient.Setup(c => c.GetMenuAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Menu());
        smsClient.Setup(c => c.SendOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await runner.RunAsync();

        console.Output.Should().Contain("\u0423\u0421\u041f\u0415\u0425");
    }

    [Fact]
    public async Task RunAsync_ShouldWriteServerError_WhenSendOrderReturnsBusinessError()
    {
        var smsClient = new Mock<ISmsClient>();
        var menuRepository = new Mock<IMenuRepository>();
        var console = new TestConsoleIO("5979224:1");
        var runner = new OrderConsoleRunner(smsClient.Object, menuRepository.Object, console);

        menuRepository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        menuRepository.Setup(r => r.SaveMenuAsync(It.IsAny<IEnumerable<MenuItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        smsClient.Setup(c => c.GetMenuAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Menu());
        smsClient.Setup(c => c.SendOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmsApiException("Order rejected"));

        await runner.RunAsync();

        console.Output.Should().Contain("Order rejected");
    }

    [Fact]
    public async Task RunAsync_ShouldWriteInfrastructureError_WhenDatabaseFails()
    {
        var smsClient = new Mock<ISmsClient>();
        var menuRepository = new Mock<IMenuRepository>();
        var console = new TestConsoleIO();
        var runner = new OrderConsoleRunner(smsClient.Object, menuRepository.Object, console);

        menuRepository.Setup(r => r.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("database unavailable"));

        await runner.RunAsync();

        console.Output.Should().Contain(message => message.Contains("database unavailable"));
        smsClient.Verify(c => c.GetMenuAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static List<MenuItem> Menu() =>
    [
        new MenuItem { Id = "5979224", Article = "A1004292", Name = "Buckwheat", Price = 50 },
        new MenuItem { Id = "9084246", Article = "A1004293", Name = "Candy", Price = 300 }
    ];

    private sealed class TestConsoleIO(params string[] input) : IConsoleIO
    {
        private readonly Queue<string> _input = new(input);

        public List<string> Output { get; } = [];

        public string? ReadLine()
        {
            return _input.Count > 0 ? _input.Dequeue() : null;
        }

        public void WriteLine(string message)
        {
            Output.Add(message);
        }
    }
}
