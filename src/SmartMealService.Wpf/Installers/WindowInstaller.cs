using Autofac;
using SmartMealService.Wpf.Windows.MainWindow;

namespace SmartMealService.Wpf.Installers;

public sealed class WindowInstaller : IContainerInstaller<WindowInstaller>
{
    public static void Install(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().InstancePerDependency();
    }
}
