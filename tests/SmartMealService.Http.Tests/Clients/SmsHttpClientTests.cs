using System.Text;
using System.Text.Json;
using FluentAssertions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SmartMealService.Http.Tests;

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
                                    "Name": "\u041a\u0430\u0448\u0430 \u0433\u0440\u0435\u0447\u043d\u0435\u0432\u0430\u044f",
                                    "Price": 50,
                                    "IsWeighted": false,
                                    "FullPath": "\u041f\u0420\u041e\u0418\u0417\u0412\u041e\u0414\u0421\u0422\u0412\u041e\\\u0413\u0430\u0440\u043d\u0438\u0440\u044b",
                                    "Barcodes": ["57890975627974236429"]
                                },
                                {
                                    "Id": "9084246",
                                    "Article": "A1004293",
                                    "Name": "\u041a\u043e\u043d\u0444\u0435\u0442\u044b \u041a\u043e\u0440\u043e\u0432\u043a\u0430",
                                    "Price": 300,
                                    "IsWeighted": true,
                                    "FullPath": "\u0414\u0415\u0421\u0415\u0420\u0422\u042b\\\u0420\u0430\u0437\u0432\u0435\u0441",
                                    "Barcodes": []
                                }
                            ]
                        }
                    }
                    """));

        var result = await _client.GetMenuAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("5979224");
        result[0].Name.Should().Be("\u041a\u0430\u0448\u0430 \u0433\u0440\u0435\u0447\u043d\u0435\u0432\u0430\u044f");
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
                        "ErrorMessage": "\u041c\u0435\u043d\u044e \u043d\u0435\u0434\u043e\u0441\u0442\u0443\u043f\u043d\u043e"
                    }
                    """));

        var act = async () => await _client.GetMenuAsync();

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("\u041c\u0435\u043d\u044e \u043d\u0435\u0434\u043e\u0441\u0442\u0443\u043f\u043d\u043e");
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
                        "ErrorMessage": "\u0417\u0430\u043a\u0430\u0437 \u043d\u0435 \u043d\u0430\u0439\u0434\u0435\u043d"
                    }
                    """));

        var order = new Order { Items = [new OrderItem { Id = "5979224", Quantity = 1 }] };

        var act = async () => await _client.SendOrderAsync(order);

        await act.Should().ThrowAsync<SmsApiException>()
            .WithMessage("\u0417\u0430\u043a\u0430\u0437 \u043d\u0435 \u043d\u0430\u0439\u0434\u0435\u043d");
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
