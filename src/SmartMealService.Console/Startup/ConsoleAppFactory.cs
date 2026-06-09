using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartMealService.Console.ConsoleIO;
using SmartMealService.Console.Ordering;
using SmartMealService.Console.Persistence;
using SmartMealService.Console.Persistence.EfCore;
using SmartMealService.Http;
using SmartMealService.Shared.Abstractions;

namespace SmartMealService.Console.Startup;

public static class ConsoleAppFactory
{
    public static ConsoleApplication Create(string basePath)
    {
        var configuration = LoadConfiguration(basePath);
        ConfigureLogging();

        var container = BuildContainer(configuration);
        return new ConsoleApplication(container);
    }

    internal static IContainer BuildContainer(IConfiguration configuration)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        builder.RegisterType<LoggingConsoleIO>().As<IConsoleIO>().SingleInstance();
        builder.RegisterType<EfMenuRepository>().As<IMenuRepository>().InstancePerLifetimeScope();
        builder.RegisterType<OrderConsoleRunner>().InstancePerLifetimeScope();

        builder.Register(context =>
            new SmsHttpClient(
                RequiredSetting(context.Resolve<IConfiguration>(), "SmsHttp:BaseUrl"),
                RequiredSetting(context.Resolve<IConfiguration>(), "SmsHttp:Username"),
                RequiredSetting(context.Resolve<IConfiguration>(), "SmsHttp:Password")))
            .As<ISmsClient>()
            .SingleInstance();

        builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var options = new DbContextOptionsBuilder<MenuDbContext>()
                    .UseNpgsql(RequiredConnectionString(configuration, "DefaultConnection"))
                    .Options;

                return new MenuDbContext(options);
            })
            .InstancePerLifetimeScope();

        return builder.Build();
    }

    private static IConfigurationRoot LoadConfiguration(string basePath) =>
        new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"logs/test-sms-console-app-{DateTime.Now:yyyyMMdd}.log")
            .CreateLogger();
    }

    private static string RequiredConnectionString(IConfiguration configuration, string name)
    {
        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Строка подключения '{name}' не настроена.");

        return value;
    }

    private static string RequiredSetting(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Параметр '{key}' не настроен.");

        return value;
    }
}
