using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartMealService.Console.ConsoleIO;
using SmartMealService.Console.Ordering;
using SmartMealService.Console.Persistence.EfCore;
using SmartMealService.Http;

namespace SmartMealService.Console.Startup;

public static class ConsoleAppFactory
{
    public static ConsoleApplication Create(string basePath)
    {
        var configuration = LoadConfiguration(basePath);
        ConfigureLogging();

        var console = new LoggingConsoleIO();
        var smsClient = CreateSmsClient(configuration);
        var dbContext = CreateMenuDbContext(configuration);
        var menuRepository = new EfMenuRepository(dbContext);
        var runner = new OrderConsoleRunner(smsClient, menuRepository, console);

        return new ConsoleApplication(runner, dbContext);
    }

    private static IConfigurationRoot LoadConfiguration(string basePath) =>
        new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File($"logs/test-sms-console-app-{DateTime.Now:yyyyMMdd}.log")
            .CreateLogger();
    }

    private static SmsHttpClient CreateSmsClient(IConfiguration configuration) =>
        new(
            RequiredSetting(configuration, "SmsHttp:BaseUrl"),
            RequiredSetting(configuration, "SmsHttp:Username"),
            RequiredSetting(configuration, "SmsHttp:Password"));

    private static MenuDbContext CreateMenuDbContext(IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseNpgsql(RequiredConnectionString(configuration, "DefaultConnection"))
            .Options;

        return new MenuDbContext(options);
    }

    private static string RequiredConnectionString(IConfiguration configuration, string name)
    {
        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Connection string '{name}' is not configured.");

        return value;
    }

    private static string RequiredSetting(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Setting '{key}' is not configured.");

        return value;
    }
}
