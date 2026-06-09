using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

app.MapGet("/health", () => Results.Ok(new { Status = "OK" }));

app.MapPost("/", async (HttpRequest request) =>
{
    if (!HasExpectedBasicAuth(request))
        return Results.Unauthorized();

    using var document = await JsonDocument.ParseAsync(request.Body);
    var command = document.RootElement.GetProperty("Command").GetString();

    return command switch
    {
        "GetMenu" when IsScenarioEnabled("FAKE_SMS_GET_MENU_SUCCESS", defaultValue: true) => Results.Json(new
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
                        Name = "Каша гречневая",
                        Price = 50,
                        IsWeighted = false,
                        FullPath = @"ПРОИЗВОДСТВО\Гарниры",
                        Barcodes = new[] { "57890975627974236429" }
                    },
                    new
                    {
                        Id = "9084246",
                        Article = "A1004293",
                        Name = "Конфеты Коровка",
                        Price = 300,
                        IsWeighted = true,
                        FullPath = @"ДЕСЕРТЫ\Развес",
                        Barcodes = Array.Empty<string>()
                    }
                }
            }
        }),
        "GetMenu" => Results.Json(new
        {
            Command = "GetMenu",
            Success = false,
            ErrorMessage = "Меню недоступно"
        }),
        "SendOrder" when IsScenarioEnabled("FAKE_SMS_SEND_ORDER_SUCCESS", defaultValue: true) =>
            Results.Json(new { Command = "SendOrder", Success = true, ErrorMessage = "" }),
        "SendOrder" => Results.Json(new
        {
            Command = "SendOrder",
            Success = false,
            ErrorMessage = "Заказ не принят"
        }),
        _ => Results.Json(new
        {
            Command = command ?? "",
            Success = false,
            ErrorMessage = $"Неизвестная команда: {command}"
        })
    };
});

app.Run();
return;

static bool HasExpectedBasicAuth(HttpRequest request)
{
    if (!request.Headers.TryGetValue("Authorization", out var values))
        return false;

    var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpass"));
    return values.Count == 1 && values[0] == $"Basic {expected}";
}

static bool IsScenarioEnabled(string variableName, bool defaultValue)
{
    var value = Environment.GetEnvironmentVariable(variableName);
    return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
}
