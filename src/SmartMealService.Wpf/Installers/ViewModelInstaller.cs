using Autofac;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;
using SmartMealService.Wpf.Windows.MainWindow;

namespace SmartMealService.Wpf.Installers;

public sealed class ViewModelInstaller : IContainerInstaller<ViewModelInstaller>
{
    public static void Install(ContainerBuilder builder)
    {
        builder.Register(context => new EnvironmentVariablesPanelViewModel(
                context.Resolve<IReadOnlyCollection<string>>(),
                context.Resolve<IEnvironmentVariableStore>(),
                context.Resolve<IEnvironmentVariableChangeLogger>(),
                context.Resolve<IReadOnlyDictionary<string, string>>(),
                context.ResolveNamed<IReadOnlyDictionary<string, string>>("EnvironmentVariableComments")))
            .SingleInstance();

        builder.RegisterType<MainWindowViewModel>().SingleInstance();
    }
}
