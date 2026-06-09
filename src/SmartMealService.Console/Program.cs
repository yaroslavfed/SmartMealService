using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartMealService.Console.Data;
using SmartMealService.Console.Services;
using SmartMealService.Http;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/test-sms-console-app-{DateTime.Now:yyyyMMdd}.log")
    .CreateLogger();

try
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    var httpBaseUrl = configuration["SmsHttp:BaseUrl"];
    var httpUsername = configuration["SmsHttp:Username"];
    var httpPassword = configuration["SmsHttp:Password"];

    if (string.IsNullOrWhiteSpace(httpBaseUrl) ||
        string.IsNullOrWhiteSpace(httpUsername) ||
        string.IsNullOrWhiteSpace(httpPassword))
    {
        throw new InvalidOperationException("SmsHttp settings are not configured.");
    }

    var dbOptions = new DbContextOptionsBuilder<MenuDbContext>()
        .UseNpgsql(connectionString)
        .Options;

    await using var dbContext = new MenuDbContext(dbOptions);
    var menuRepository = new MenuRepository(dbContext);
    await menuRepository.InitializeAsync();

    var smsClient = new SmsHttpClient(httpBaseUrl, httpUsername, httpPassword);

    List<MenuItem> menuItems;
    try
    {
        menuItems = await smsClient.GetMenuAsync();
    }
    catch (SmsApiException ex)
    {
        WriteLine(ex.Message);
        return;
    }

    await menuRepository.SaveMenuAsync(menuItems);

    foreach (var item in menuItems)
        WriteLine($"{item.Name} - {item.Id} ({item.Article}) - {item.Price}");

    var order = new Order();
    while (true)
    {
        WriteLine("\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0431\u043b\u044e\u0434\u0430 \u0432 \u0444\u043e\u0440\u043c\u0430\u0442\u0435 \u041a\u043e\u04341:\u041a\u043e\u043b\u0438\u0447\u0435\u0441\u0442\u0432\u043e1;\u041a\u043e\u04342:\u041a\u043e\u043b\u0438\u0447\u0435\u0441\u0442\u0432\u043e2");
        var input = System.Console.ReadLine();

        try
        {
            order.Items.AddRange(OrderInputParser.Parse(input, menuItems));
            break;
        }
        catch (OrderInputException ex)
        {
            WriteLine(ex.Message);
        }
    }

    try
    {
        await smsClient.SendOrderAsync(order);
        WriteLine("\u0423\u0421\u041f\u0415\u0425");
    }
    catch (SmsApiException ex)
    {
        WriteLine(ex.Message);
    }
}
catch (Exception ex)
{
    WriteLine(ex.Message);
}
finally
{
    await Log.CloseAndFlushAsync();
}

static void WriteLine(string message)
{
    System.Console.WriteLine(message);
    Log.Information("{Message}", message);
}
