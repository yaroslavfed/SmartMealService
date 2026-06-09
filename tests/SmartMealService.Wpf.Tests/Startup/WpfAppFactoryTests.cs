using System.IO;
using Autofac;
using FluentAssertions;
using SmartMealService.Wpf.Services;
using SmartMealService.Wpf.Startup;
using SmartMealService.Wpf.ViewModels;

namespace SmartMealService.Wpf.Tests.Startup;

public class WpfAppFactoryTests
{
    [Fact]
    public void BuildServices_ShouldRegisterWpfDependencies()
    {
        var firstName = $"SMART_MEAL_SERVICE_WPF_TEST_{Guid.NewGuid():N}_1";
        var secondName = $"SMART_MEAL_SERVICE_WPF_TEST_{Guid.NewGuid():N}_2";
        var logDirectory = Path.Combine(Path.GetTempPath(), "smart-meal-wpf-tests", Guid.NewGuid().ToString("N"));

        using var provider = WpfAppFactory.BuildServices(
            [firstName, secondName],
            logDirectory);

        try
        {
            provider.Resolve<IEnvironmentVariableStore>().Should().BeOfType<UserEnvironmentVariableStore>();
            provider.Resolve<IEnvironmentVariableChangeLogger>().Should().BeOfType<NLogEnvironmentVariableChangeLogger>();
            provider.Resolve<MainViewModel>().EnvironmentVariables.Should().HaveCount(2);

            RunInStaThread(() =>
            {
                var window = provider.Resolve<MainWindow>();
                window.DataContext.Should().BeOfType<MainViewModel>();
                window.Close();
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable(firstName, null, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(secondName, null, EnvironmentVariableTarget.User);
        }
    }

    [Fact]
    public void LoadEnvironmentVariableNames_ShouldReadNamesFromAppsettings()
    {
        var basePath = CreateConfigurationDirectory();

        var names = WpfAppFactory.LoadEnvironmentVariableNames(basePath);

        names.Should().Equal("SMS_HTTP_BASE_URL", "SMS_HTTP_USERNAME", "SMS_HTTP_PASSWORD");
    }

    [Fact]
    public void LoadEnvironmentVariableOptions_ShouldReadCommentsFromAppsettings()
    {
        var basePath = CreateConfigurationDirectory();

        var options = WpfAppFactory.LoadEnvironmentVariableOptions(basePath);

        options.Comments.Should().ContainKey("SMS_HTTP_BASE_URL")
            .WhoseValue.Should().Be("Адрес SMS HTTP-сервера");
    }

    [Fact]
    public void BuildLogFilePath_ShouldUseRequiredFileNameFormat()
    {
        var date = new DateTime(2026, 6, 10);

        var path = WpfAppFactory.BuildLogFilePath("logs", date);

        path.Should().EndWith(@"logs\test-sms-wpf-app-20260610.log");
    }

    private static string CreateConfigurationDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "smart-meal-wpf-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, "appsettings.json"),
            """
            {
              "EnvironmentVariables": {
                "Names": [
                  "SMS_HTTP_BASE_URL",
                  "SMS_HTTP_USERNAME",
                  "SMS_HTTP_PASSWORD"
                ],
                "Comments": {
                  "SMS_HTTP_BASE_URL": "Адрес SMS HTTP-сервера"
                }
              }
            }
            """);

        return directory;
    }

    private static void RunInStaThread(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
            throw exception;
    }
}
