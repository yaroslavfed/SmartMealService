using System.Text;
using System.Text.Json;
using FluentAssertions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SmartMealService.Http.Tests.Clients;

public class SmsHttpClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly SmsHttpClient _client;

    public SmsHttpClientTests()
    {
        _server = WireMockServer.Start();
        _client = new SmsHttpClient(_server.Url!, "testuser", "testpass");
    }

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task GetMenu_ShouldReturnMenuItems_WhenServerReturnsSuccess()
    {
        _server
            .Given(Request.Create().WithPath("/").WithBody(body => body!.Contains("GetMenu")).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "GetMenu",
                        "Success": true,
                        "ErrorMessage": "",
                        "Data": {
                            "MenuItems": [
                                {
                                    "Id": "5979224",
                                    "Article": "A1004292",
                                    "Name": "Каша гречневая",
                                    "Price": 50,
                                    "IsWeighted": false,
                                    "FullPath": "ПРОИЗВОДСТВО\\Гарниры",
                                    "Barcodes": ["57890975627974236429"]
                                },
                                {
                                    "Id": "9084246",
                                    "Article": "A1004293",
                                    "Name": "Конфеты Коровка",
                                    "Price": 300,
                                    "IsWeighted": true,
                                    "FullPath": "ДЕСЕРТЫ\\Развес",
                                    "Barcodes": []
                                }
                            ]
                        }
                    }
                    """));

        var result = await _client.GetMenuAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("5979224");
        result[0].Name.Should().Be("Каша гречневая");
        result[0].Price.Should().Be(50m);
        result[1].Id.Should().Be("9084246");
        result[1].IsWeighted.Should().BeTrue();
    }

    [Fact]
    public async Task GetMenu_ShouldThrowException_WhenServerReturnsSuccessFalse()
    {
        _server
            .Given(Request.Create().WithPath("/").WithBody(body => body!.Contains("GetMenu")).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "GetMenu",
                        "Success": false,
                        "ErrorMessage": "Меню недоступно"
                    }
                    """));

        var act = async () => await _client.GetMenuAsync();

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("Меню недоступно");
    }

    [Fact]
    public async Task GetMenu_ShouldThrowHttpRequestException_WhenServerReturnsHttpError()
    {
        _server
            .Given(Request.Create().WithPath("/").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        var act = async () => await _client.GetMenuAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetMenu_ShouldThrowJsonException_WhenServerReturnsMalformedJson()
    {
        _server
            .Given(Request.Create().WithPath("/").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{ malformed json"));

        var act = async () => await _client.GetMenuAsync();

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GetMenu_ShouldSendBasicAuth_WithCorrectCredentials()
    {
        _server
            .Given(Request.Create().WithPath("/").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "GetMenu",
                        "Success": true,
                        "ErrorMessage": "",
                        "Data": { "MenuItems": [] }
                    }
                    """));

        await _client.GetMenuAsync();

        var authHeader = _server.LogEntries.Last().RequestMessage!.Headers!["Authorization"];
        authHeader.Should()
            .ContainSingle()
            .Which.Should()
            .Be($"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpass"))}");
    }

    [Fact]
    public async Task GetMenu_ShouldSendCorrectRequestBody()
    {
        _server
            .Given(Request.Create().WithPath("/").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "GetMenu",
                        "Success": true,
                        "ErrorMessage": "",
                        "Data": { "MenuItems": [] }
                    }
                    """));

        await _client.GetMenuAsync();

        var requestBody = _server.LogEntries.Last().RequestMessage?.Body;
        using var document = JsonDocument.Parse(requestBody!);
        var root = document.RootElement;

        root.GetProperty("Command").GetString().Should().Be("GetMenu");
        root.GetProperty("CommandParameters").GetProperty("WithPrice").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task SendOrder_ShouldReturnTrue_WhenServerReturnsSuccess()
    {
        _server
            .Given(Request.Create().WithPath("/").WithBody(body => body!.Contains("SendOrder")).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "SendOrder",
                        "Success": true,
                        "ErrorMessage": ""
                    }
                    """));

        var order = new Order { Items = [new OrderItem { Id = "5979224", Quantity = 1 }] };

        var result = await _client.SendOrderAsync(order);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendOrder_ShouldThrowException_WhenServerReturnsSuccessFalse()
    {
        _server
            .Given(Request.Create().WithPath("/").WithBody(body => body!.Contains("SendOrder")).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "SendOrder",
                        "Success": false,
                        "ErrorMessage": "Заказ не найден"
                    }
                    """));

        var order = new Order { Items = [new OrderItem { Id = "5979224", Quantity = 1 }] };

        var act = async () => await _client.SendOrderAsync(order);

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("Заказ не найден");
    }

    [Fact]
    public async Task SendOrder_ShouldSendCorrectRequestBody()
    {
        _server
            .Given(Request.Create().WithPath("/").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    """
                    {
                        "Command": "SendOrder",
                        "Success": true,
                        "ErrorMessage": ""
                    }
                    """));

        var order = new Order { Id = "62137983-1117-4D10-87C1-EF40A4348250" };
        order.Items.Add(new OrderItem { Id = "5979224", Quantity = 1 });
        order.Items.Add(new OrderItem { Id = "9084246", Quantity = 0.408 });

        await _client.SendOrderAsync(order);

        var requestBody = _server.LogEntries.Last().RequestMessage?.Body;
        using var document = JsonDocument.Parse(requestBody!);
        var root = document.RootElement;
        var parameters = root.GetProperty("CommandParameters");
        var menuItems = parameters.GetProperty("MenuItems").EnumerateArray().ToList();

        root.GetProperty("Command").GetString().Should().Be("SendOrder");
        parameters.GetProperty("OrderId").GetString().Should().Be("62137983-1117-4D10-87C1-EF40A4348250");
        menuItems.Should().HaveCount(2);
        menuItems[0].GetProperty("Id").GetString().Should().Be("5979224");
        menuItems[0].GetProperty("Quantity").GetString().Should().Be("1");
        menuItems[1].GetProperty("Id").GetString().Should().Be("9084246");
        menuItems[1].GetProperty("Quantity").GetString().Should().Be("0.408");
    }
}
