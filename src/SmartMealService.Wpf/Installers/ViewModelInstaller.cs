using Autofac;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;
using SmartMealService.Wpf.Windows.MainWindow;

namespace SmartMealService.Wpf.Installers;

public sealed class ViewModelInstaller : IContainerInstaller<ViewModelInstaller>
{
    public static void Install(ContainerBuilder builder)
    {
        builder.RegisterType<EnvironmentVariablesPanelViewModel>().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().SingleInstance();
    }
}
