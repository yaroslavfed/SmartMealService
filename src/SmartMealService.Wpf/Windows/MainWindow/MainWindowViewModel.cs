using ReactiveUI;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;

namespace SmartMealService.Wpf.Windows.MainWindow;

public sealed class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel(EnvironmentVariablesPanelViewModel environmentVariablesPanel)
    {
        EnvironmentVariablesPanel = environmentVariablesPanel;
    }

    public EnvironmentVariablesPanelViewModel EnvironmentVariablesPanel { get; }
}
