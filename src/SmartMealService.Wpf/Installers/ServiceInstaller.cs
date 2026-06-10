using Autofac;
using SmartMealService.Wpf.Services.EnvironmentVariables;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableChangeNotifier;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;

namespace SmartMealService.Wpf.Installers;

public sealed class ServiceInstaller : IContainerInstaller<ServiceInstaller>
{
    public static void Install(ContainerBuilder builder)
    {
        builder.RegisterType<WindowsEnvironmentVariableChangeNotifier>()
            .As<IEnvironmentVariableChangeNotifier>()
            .SingleInstance();

        builder.RegisterType<UserEnvironmentVariableStore>()
            .As<IEnvironmentVariableStore>()
            .SingleInstance();

        builder.RegisterType<NLogEnvironmentVariableChangeLogger>()
            .As<IEnvironmentVariableChangeLogger>()
            .SingleInstance();
    }
}
