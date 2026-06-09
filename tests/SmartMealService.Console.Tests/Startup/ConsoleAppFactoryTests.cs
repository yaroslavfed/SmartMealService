using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SmartMealService.Console.ConsoleIO;
using SmartMealService.Console.Ordering;
using SmartMealService.Console.Persistence;
using SmartMealService.Console.Persistence.EfCore;
using SmartMealService.Console.Startup;
using SmartMealService.Shared.Abstractions;

namespace SmartMealService.Console.Tests.Startup;

public class ConsoleAppFactoryTests
{
    [Fact]
    public void BuildContainer_ShouldRegisterConsoleDependencies()
    {
        using var container = ConsoleAppFactory.BuildContainer(ValidConfiguration());

        container.Resolve<IConsoleIO>().Should().BeOfType<LoggingConsoleIO>();
        container.Resolve<ISmsClient>().Should().NotBeNull();
        container.Resolve<IMenuRepository>().Should().BeOfType<EfMenuRepository>();
        container.Resolve<MenuDbContext>().Should().NotBeNull();
        container.Resolve<OrderConsoleRunner>().Should().NotBeNull();
    }

    [Fact]
    public void BuildContainer_ShouldThrow_WhenRequiredSmsSettingIsMissing()
    {
        var settings = ValidSettings();
        settings.Remove("SmsHttp:Username");
        using var container = ConsoleAppFactory.BuildContainer(Configuration(settings));

        var act = () => container.Resolve<ISmsClient>();

        act.Should().Throw<DependencyResolutionException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*SmsHttp:Username*");
    }

    private static IConfiguration ValidConfiguration() => Configuration(ValidSettings());

    private static Dictionary<string, string?> ValidSettings() =>
        new()
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=smart_meal_service;Username=postgres;Password=postgres",
            ["SmsHttp:BaseUrl"] = "http://localhost:5000/",
            ["SmsHttp:Username"] = "testuser",
            ["SmsHttp:Password"] = "testpass"
        };

    private static IConfiguration Configuration(Dictionary<string, string?> settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
}
