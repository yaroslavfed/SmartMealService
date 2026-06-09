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
    private readonly SmsHttpClient  _client;

    public SmsHttpClientTests()
    {
        _server = WireMockServer.Start();
        _client = new SmsHttpClient(_server.Url!, "testuser", "testpass");
    }

    public void Dispose() => _server.Stop();

    // --- GetMenu ---

    [Fact]
    public async Task GetMenu_ShouldReturnMenuItems_WhenServerReturnsSuccess()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .WithBody(body => body!.Contains("GetMenu"))
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
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
                        """
                    )
            );

        var result = await _client.GetMenuAsync();

        result
            .Should()
            .HaveCount(2);
        result[0]
            .Id.Should()
            .Be("5979224");
        result[0]
            .Name.Should()
            .Be("Каша гречневая");
        result[0]
            .Price.Should()
            .Be(50m);
        result[1]
            .Id.Should()
            .Be("9084246");
        result[1]
            .IsWeighted.Should()
            .BeTrue();
    }

    [Fact]
    public async Task GetMenu_ShouldThrowException_WhenServerReturnsSuccessFalse()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .WithBody(body => body!.Contains("GetMenu"))
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(
                        """
                        {
                            "Command": "GetMenu",
                            "Success": false,
                            "ErrorMessage": "Меню недоступно"
                        }
                        """
                    )
            );

        var act = async () => await _client.GetMenuAsync();

        await act
              .Should()
              .ThrowAsync<SmsApiException>()
              .WithMessage("Меню недоступно");
    }

    [Fact]
    public async Task GetMenu_ShouldSendBasicAuth_WithCorrectCredentials()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
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
                        """
                    )
            );

        await _client.GetMenuAsync();

        var authHeader = _server.LogEntries.Last()
                                .RequestMessage!.Headers!["Authorization"];
        authHeader
            .Should()
            .ContainMatch("Basic *");
    }

    [Fact]
    public async Task GetMenu_ShouldSendCorrectRequestBody()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
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
                        """
                    )
            );

        await _client.GetMenuAsync();

        var requestBody = _server.LogEntries.Last()
                                 .RequestMessage?.Body;
        requestBody
            .Should()
            .Contain("\"Command\"");
        requestBody
            .Should()
            .Contain("GetMenu");
        requestBody
            .Should()
            .Contain("WithPrice");
    }

    // --- SendOrder ---

    [Fact]
    public async Task SendOrder_ShouldReturnTrue_WhenServerReturnsSuccess()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .WithBody(body => body!.Contains("SendOrder"))
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(
                        """
                        {
                            "Command": "SendOrder",
                            "Success": true,
                            "ErrorMessage": ""
                        }
                        """
                    )
            );

        var order = new Order { Items = [new OrderItem { Id = "5979224", Quantity = 1 }] };

        var result = await _client.SendOrderAsync(order);

        result
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task SendOrder_ShouldThrowException_WhenServerReturnsSuccessFalse()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .WithBody(body => body!.Contains("SendOrder"))
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(
                        """
                        {
                            "Command": "SendOrder",
                            "Success": false,
                            "ErrorMessage": "Заказ не найден"
                        }
                        """
                    )
            );

        var order = new Order { Items = [new OrderItem { Id = "5979224", Quantity = 1 }] };

        var act = async () => await _client.SendOrderAsync(order);

        await act
              .Should()
              .ThrowAsync<SmsApiException>()
              .WithMessage("Заказ не найден");
    }

    [Fact]
    public async Task SendOrder_ShouldSendCorrectRequestBody()
    {
        _server
            .Given(
                Request
                    .Create()
                    .WithPath("/")
                    .UsingPost()
            )
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(
                        """
                        {
                            "Command": "SendOrder",
                            "Success": true,
                            "ErrorMessage": ""
                        }
                        """
                    )
            );

        var order = new Order { Id = "62137983-1117-4D10-87C1-EF40A4348250" };
        order.Items.Add(new OrderItem { Id = "5979224", Quantity = 1 });
        order.Items.Add(new OrderItem { Id = "9084246", Quantity = 0.408 });

        await _client.SendOrderAsync(order);

        var requestBody = _server.LogEntries.Last()
                                 .RequestMessage?.Body;
        requestBody
            .Should()
            .Contain("SendOrder");
        requestBody
            .Should()
            .Contain("62137983-1117-4D10-87C1-EF40A4348250");
        requestBody
            .Should()
            .Contain("5979224");
        requestBody
            .Should()
            .Contain("9084246");
    }
}