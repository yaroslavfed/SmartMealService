using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Targets;
using SmartMealService.Wpf.Configuration;
using SmartMealService.Wpf.Services;
using SmartMealService.Wpf.ViewModels;

namespace SmartMealService.Wpf.Startup;

public static class WpfAppFactory
{
    public static IContainer Create(string basePath)
    {
        var options = LoadEnvironmentVariableOptions(basePath);
        return BuildServices(options.Names, Path.Combine(basePath, "logs"), options.Comments);
    }

    public static IContainer BuildServices(
        IReadOnlyCollection<string> environmentVariableNames,
        string logDirectory,
        IReadOnlyDictionary<string, string>? comments = null)
    {
        ReactiveUiBootstrapper.EnsureInitialized();
        ConfigureLogging(logDirectory);

        var builder = new ContainerBuilder();
        builder.RegisterInstance(environmentVariableNames).As<IReadOnlyCollection<string>>().SingleInstance();
        builder.RegisterInstance(comments ?? new Dictionary<string, string>())
            .As<IReadOnlyDictionary<string, string>>()
            .SingleInstance();
        builder.RegisterType<UserEnvironmentVariableStore>().As<IEnvironmentVariableStore>().SingleInstance();
        builder.RegisterType<NLogEnvironmentVariableChangeLogger>().As<IEnvironmentVariableChangeLogger>().SingleInstance();
        builder.RegisterType<MainViewModel>().SingleInstance();
        builder.RegisterType<MainWindow>().InstancePerDependency();

        return builder.Build();
    }

    public static IReadOnlyList<string> LoadEnvironmentVariableNames(string basePath)
    {
        return LoadEnvironmentVariableOptions(basePath).Names;
    }

    public static EnvironmentVariableOptions LoadEnvironmentVariableOptions(string basePath)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        return EnvironmentVariableOptions.FromConfiguration(configuration);
    }

    public static string BuildLogFilePath(string logDirectory, DateTime date)
    {
        return Path.Combine(logDirectory, $"test-sms-wpf-app-{date:yyyyMMdd}.log");
    }

    private static void ConfigureLogging(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);

        var fileTarget = new FileTarget("wpf-log-file")
        {
            FileName = BuildLogFilePath(logDirectory, DateTime.Now),
            Layout = "${longdate} [${level:uppercase=true}] ${message}"
        };

        var configuration = new LoggingConfiguration();
        configuration.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
        LogManager.Configuration = configuration;
    }
}
