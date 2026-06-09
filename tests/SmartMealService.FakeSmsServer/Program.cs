using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

app.MapGet("/health", () => Results.Ok(new { Status = "OK" }));

app.MapPost(
    "/",
    async (HttpRequest request) =>
    {
        if (!HasExpectedBasicAuth(request))
            return Results.Unauthorized();

        using var document = await JsonDocument.ParseAsync(request.Body);
        var command = document
                      .RootElement.GetProperty("Command")
                      .GetString();

        return command switch
        {
            "GetMenu" when IsScenarioEnabled("FAKE_SMS_GET_MENU_SUCCESS", defaultValue: true) => Results.Json(
                new
                {
                    Command = "GetMenu",
                    Success = true,
                    ErrorMessage = "",
                    Data = new
                    {
                        MenuItems = new[]
                        {
                            new
                            {
                                Id = "5979224",
                                Article = "A1004292",
                                Name
                                    = "\u041a\u0430\u0448\u0430 \u0433\u0440\u0435\u0447\u043d\u0435\u0432\u0430\u044f",
                                Price = 50,
                                IsWeighted = false,
                                FullPath
                                    = "\u041f\u0420\u041e\u0418\u0417\u0412\u041e\u0414\u0421\u0422\u0412\u041e\\\u0413\u0430\u0440\u043d\u0438\u0440\u044b",
                                Barcodes = new[]
                                {
                                    "57890975627974236429"
                                }
                            },
                            new
                            {
                                Id = "9084246",
                                Article = "A1004293",
                                Name
                                    = "\u041a\u043e\u043d\u0444\u0435\u0442\u044b \u041a\u043e\u0440\u043e\u0432\u043a\u0430",
                                Price = 300,
                                IsWeighted = true,
                                FullPath
                                    = "\u0414\u0415\u0421\u0415\u0420\u0422\u042b\\\u0420\u0430\u0437\u0432\u0435\u0441",
                                Barcodes = Array.Empty<string>()
                            }
                        }
                    }
                }
            ),
            "GetMenu" => Results.Json(
                new
                {
                    Command = "GetMenu",
                    Success = false,
                    ErrorMessage = "Fake GetMenu error"
                }
            ),
            "SendOrder" when IsScenarioEnabled("FAKE_SMS_SEND_ORDER_SUCCESS", defaultValue: true) =>
                Results.Json(new { Command = "SendOrder", Success = true, ErrorMessage = "" }),
            "SendOrder" => Results.Json(
                new
                {
                    Command = "SendOrder",
                    Success = false,
                    ErrorMessage = "Fake SendOrder error"
                }
            ),
            _ => Results.Json(
                new { Command = command ?? "", Success = false, ErrorMessage = $"Unknown command: {command}" }
            )
        };
    }
);

app.Run();
return;

static bool HasExpectedBasicAuth(HttpRequest request)
{
    if (!request.Headers.TryGetValue("Authorization", out var values))
        return false;

    var expected = Convert.ToBase64String("testuser:testpass"u8.ToArray());
    return values.Count == 1 && values[0] == $"Basic {expected}";
}

static bool IsScenarioEnabled(string variableName, bool defaultValue)
{
    var value = Environment.GetEnvironmentVariable(variableName);
    return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
}
