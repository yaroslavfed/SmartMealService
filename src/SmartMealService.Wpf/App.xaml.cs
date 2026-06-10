using System.Windows;
using Autofac;
using NLog;
using SmartMealService.Wpf.Startup;
using SmartMealService.Wpf.Windows.MainWindow;

namespace SmartMealService.Wpf;

public partial class App : Application
{
    private IContainer? _container;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _container = WpfAppFactory.Create(AppContext.BaseDirectory);
        var mainWindow = _container.Resolve<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        LogManager.Shutdown();
        base.OnExit(e);
    }
}
