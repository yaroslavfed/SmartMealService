using System.IO;
using Autofac;
using FluentAssertions;
using NLog;
using SmartMealService.Wpf.Startup;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;
using SmartMealService.Wpf.Services.EnvironmentVariables;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableChangeNotifier;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;
using SmartMealService.Wpf.Windows.MainWindow;

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
            provider.Resolve<IEnvironmentVariableChangeNotifier>().Should().BeOfType<WindowsEnvironmentVariableChangeNotifier>();
            provider.Resolve<IEnvironmentVariableChangeLogger>().Should().BeOfType<NLogEnvironmentVariableChangeLogger>();
            provider.Resolve<EnvironmentVariablesPanelViewModel>().EnvironmentVariables.Should().HaveCount(2);
            provider.Resolve<MainWindowViewModel>().EnvironmentVariablesPanel
                .Should().BeSameAs(provider.Resolve<EnvironmentVariablesPanelViewModel>());

            RunInStaThread(() =>
            {
                var window = provider.Resolve<MainWindow>();
                window.DataContext.Should().BeOfType<MainWindowViewModel>();
                window.ViewModel.Should().BeOfType<MainWindowViewModel>();
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
        var basePath = WpfProjectDirectory();

        var names = WpfAppFactory.LoadEnvironmentVariableNames(basePath);

        names.Should().NotBeEmpty();
        names.Should().OnlyContain(name => !string.IsNullOrWhiteSpace(name));
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void LoadEnvironmentVariableOptions_ShouldReadCommentsFromAppsettings()
    {
        var basePath = WpfProjectDirectory();

        var options = WpfAppFactory.LoadEnvironmentVariableOptions(basePath);

        options.Comments.Keys.Should().OnlyContain(name => options.Names.Contains(name));
        options.Comments.Values.Should().OnlyContain(comment => !string.IsNullOrWhiteSpace(comment));
        options.Defaults.Keys.Should().OnlyContain(name => options.Names.Contains(name));
    }

    [Fact]
    public void BuildLogFilePath_ShouldUseRequiredFileNameFormat()
    {
        var date = new DateTime(2026, 6, 10);

        var path = WpfAppFactory.BuildLogFilePath("logs", date);

        path.Should().EndWith(@"logs\test-sms-wpf-app-20260610.log");
    }

    [Fact]
    public void BuildServices_ShouldConfigureNLogFile()
    {
        var name = $"SMART_MEAL_SERVICE_WPF_TEST_{Guid.NewGuid():N}";
        var logDirectory = Path.Combine(Path.GetTempPath(), "smart-meal-wpf-tests", Guid.NewGuid().ToString("N"));

        using var provider = WpfAppFactory.BuildServices([name], logDirectory);

        try
        {
            var logger = provider.Resolve<IEnvironmentVariableChangeLogger>();

            logger.LogChanged(name, "test-value");
            LogManager.Flush();
            LogManager.Shutdown();

            var logFilePath = WpfAppFactory.BuildLogFilePath(logDirectory, DateTime.Now);
            File.Exists(logFilePath).Should().BeTrue();
            File.ReadAllText(logFilePath)
                .Should().Contain(name)
                .And.Contain("test-value");
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.User);
            LogManager.Shutdown();
        }
    }

    private static string WpfProjectDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var appsettingsPath = Path.Combine(
                current.FullName,
                "src",
                "SmartMealService.Wpf",
                "Properties",
                "appsettings.json");

            if (File.Exists(appsettingsPath))
                return Path.GetDirectoryName(appsettingsPath)!;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate src/SmartMealService.Wpf/Properties/appsettings.json.");
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
