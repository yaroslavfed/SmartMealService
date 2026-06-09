using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
using GrpcMenuItem = Sms.Test.MenuItem;
using GrpcOrder = Sms.Test.Order;
using SmsTestService = Sms.Test.SmsTestService;

namespace SmartMealService.Grpc.Tests;

public class SmsGrpcClientTests
{
    private readonly Mock<SmsTestService.SmsTestServiceClient> _mockGrpcClient;
    private readonly SmsGrpcClient _client;

    public SmsGrpcClientTests()
    {
        _mockGrpcClient = new Mock<SmsTestService.SmsTestServiceClient>();
        _client = new SmsGrpcClient(_mockGrpcClient.Object);
    }

    private static AsyncUnaryCall<T> MakeCall<T>(T response) =>
        new(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    // --- GetMenu ---

    [Fact]
    public async Task GetMenu_ShouldReturnMenuItems_WhenServerReturnsSuccess()
    {
        var grpcResponse = new Sms.Test.GetMenuResponse
        {
            Success = true,
            ErrorMessage = "",
            MenuItems =
            {
                new GrpcMenuItem
                {
                    Id = "5979224",
                    Article = "A1004292",
                    Name = "Каша гречневая",
                    Price = 50.0,
                    IsWeighted = false,
                    FullPath = @"ПРОИЗВОДСТВО\Гарниры",
                    Barcodes = { "57890975627974236429" }
                },
                new GrpcMenuItem
                {
                    Id = "9084246",
                    Article = "A1004293",
                    Name = "Конфеты Коровка",
                    Price = 300.0,
                    IsWeighted = true,
                    FullPath = @"ДЕСЕРТЫ\Развес"
                }
            }
        };

        _mockGrpcClient
            .Setup(c => c.GetMenuAsync(
                It.IsAny<BoolValue>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(MakeCall(grpcResponse));

        var result = await _client.GetMenuAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("5979224");
        result[0].Name.Should().Be("Каша гречневая");
        result[0].Price.Should().Be(50m);
        result[1].IsWeighted.Should().BeTrue();
    }

    [Fact]
    public async Task GetMenu_ShouldThrowSmsApiException_WhenServerReturnsSuccessFalse()
    {
        var grpcResponse = new Sms.Test.GetMenuResponse
        {
            Success = false,
            ErrorMessage = "Меню недоступно"
        };

        _mockGrpcClient
            .Setup(c => c.GetMenuAsync(
                It.IsAny<BoolValue>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(MakeCall(grpcResponse));

        var act = async () => await _client.GetMenuAsync();

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("Меню недоступно");
    }

    // --- SendOrder ---

    [Fact]
    public async Task SendOrder_ShouldReturnTrue_WhenServerReturnsSuccess()
    {
        var grpcResponse = new Sms.Test.SendOrderResponse
        {
            Success = true,
            ErrorMessage = ""
        };

        _mockGrpcClient
            .Setup(c => c.SendOrderAsync(
                It.IsAny<GrpcOrder>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(MakeCall(grpcResponse));

        var order = new Order
        {
            Items = [new OrderItem { Id = "5979224", Quantity = 1 }]
        };

        var result = await _client.SendOrderAsync(order);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendOrder_ShouldThrowSmsApiException_WhenServerReturnsSuccessFalse()
    {
        var grpcResponse = new Sms.Test.SendOrderResponse
        {
            Success = false,
            ErrorMessage = "Заказ не найден"
        };

        _mockGrpcClient
            .Setup(c => c.SendOrderAsync(
                It.IsAny<GrpcOrder>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(MakeCall(grpcResponse));

        var order = new Order
        {
            Items = [new OrderItem { Id = "5979224", Quantity = 1 }]
        };

        var act = async () => await _client.SendOrderAsync(order);

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("Заказ не найден");
    }

    [Fact]
    public async Task SendOrder_ShouldPassAllOrderItems_ToGrpcClient()
    {
        var grpcResponse = new Sms.Test.SendOrderResponse { Success = true };
        GrpcOrder? capturedRequest = null;

        _mockGrpcClient
            .Setup(c => c.SendOrderAsync(
                It.IsAny<GrpcOrder>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Callback<GrpcOrder, Metadata, DateTime?, CancellationToken>(
                (req, _, _, _) => capturedRequest = req)
            .Returns(MakeCall(grpcResponse));

        var order = new Order { Id = "62137983-1117-4D10-87C1-EF40A4348250" };
        order.Items.Add(new OrderItem { Id = "5979224", Quantity = 1 });
        order.Items.Add(new OrderItem { Id = "9084246", Quantity = 0.408 });

        await _client.SendOrderAsync(order);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Id.Should().Be("62137983-1117-4D10-87C1-EF40A4348250");
        capturedRequest.OrderItems.Should().HaveCount(2);
        capturedRequest.OrderItems[0].Id.Should().Be("5979224");
        capturedRequest.OrderItems[0].Quantity.Should().Be(1);
        capturedRequest.OrderItems[1].Id.Should().Be("9084246");
        capturedRequest.OrderItems[1].Quantity.Should().BeApproximately(0.408, 0.0001);
    }
}
